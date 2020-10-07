using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Newtonsoft.Json;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletsCrypto.Application.Handlers.UnSpentTransaction;
using WalletsCrypto.Application.Services.Address;
using WalletsCrypto.Common.Exceptions;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.Core;
using WalletsCrypto.Domain.Persistence;
using WalletsCrypto.Domain.PubSub;
using WalletsCrypto.Domain.SharedKernel;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.Domain.UserModule;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.Infrastructure.Providers;
using WalletsCrypto.ReadModel.Persistence;
using UnspentTransactionReadModel = WalletsCrypto.ReadModel.UnspentTransaction.UnspentTransaction;

namespace WalletsCrypto.Application.Services.Transaction
{
    public class TransactionWriter : AggregateBase<AddressId>, ITransactionWriter
    {
        private readonly ITransientDomainEventSubscriber _subscriber;
        private readonly IRepository<Domain.TransactionModule.Transaction, TransactionId> _transactionRepository;
        private readonly IRepository<Domain.AddressModule.Address, AddressId> _addressRepository;
        private readonly IRepository<Domain.UserModule.User,UserId> _userRepository;
        private readonly IEnumerable<IDomainEventHandler<TransactionId, TransactionCreatedEvent>> _transactionCreatedEventHandlers;
        private readonly IAddressWriter _addressWriter;
        private readonly ICacheStorage _cacheStorage;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IUnSpentTransactionUpdater _unSpentTransactionUpdater;
        private readonly IRepository<UnspentTransactionReadModel> _unspentTransactionRepository;

        public TransactionWriter(
            ITransientDomainEventSubscriber subscriber,
            IRepository<Domain.TransactionModule.Transaction, TransactionId> transactionRepository,
            IEnumerable<IDomainEventHandler<TransactionId, TransactionCreatedEvent>> transactionCreatedEventHandlers,
            IRepository<Domain.AddressModule.Address, AddressId> addressRepository,
            IRepository<Domain.UserModule.User, UserId> userRepository,
            IAddressWriter addressWriter,
            ICacheStorage cacheStorage,
            IRepository<UnspentTransactionReadModel> unspentTransactionRepository)
        {
            _subscriber = subscriber;
            _cacheStorage = cacheStorage;
            _addressWriter = addressWriter;
            _transactionRepository = transactionRepository;
            _addressRepository = addressRepository;
            _userRepository = userRepository;
            _transactionCreatedEventHandlers = transactionCreatedEventHandlers;
            _unspentTransactionRepository = unspentTransactionRepository;
            _unSpentTransactionUpdater = new UnSpentTransactionUpdater(_unspentTransactionRepository);

        }

        public async Task ReAddUnspentTx(string addressId, string hash, decimal value, int index)
        {
            var unspentTransaction = new UnspentTransaction
            {
                Hash = hash,
                Value = value,
                Index = index
            };

            await _addressWriter.AddUnspentTransactionAsync(addressId, unspentTransaction);
        }

        public async Task<(string, string)> CreateAsync(string userId, string addressId, string transactionAddress, decimal transactionAmount, TransactionTypes transactionType = TransactionTypes.Debit, int? index = null, string transactionHash = null)
        {
            return transactionType switch
            {
                TransactionTypes.Credit => await CreateCreditTransactionAsync(userId, addressId, transactionAddress, transactionAmount, index, transactionHash),
                TransactionTypes.Debit => await CreateDebitTransactionAsync(userId, addressId, transactionAddress, transactionAmount),
                _ => throw new ArgumentOutOfRangeException(nameof(transactionType)),
            };
        }

        private async Task<(string, string)> CreateCreditTransactionAsync(string userId, string addressId, string creditAddress, decimal transactionAmount, int? index = null, string transactionHash = null)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(userId));
            var address = await _addressRepository.GetByIdAsync(new AddressId(addressId));
            var transaction = new Domain.TransactionModule.Transaction(TransactionId.NewTransactionId(), address.Id, user.Id,
                CryptoCurrency.NewCryptoCurrency(transactionAmount), CreditingAddress.NewCreditingAddress(creditAddress), TransactionType.Cedit);
            _subscriber.Subscribe<TransactionCreatedEvent>(async @event => await HandleAsync(_transactionCreatedEventHandlers, @event));
            await _transactionRepository.SaveAsync(transaction);

            await _addressWriter.UpdateBalanceAsync(address.Id.IdAsStringWithoutPrefix(), transaction.GetTransactionAmount(), TransactionTypes.Credit);
            if(index.HasValue && !string.IsNullOrWhiteSpace(transactionHash))
                await _addressWriter.AddUnspentTransactionAsync(address.Id.IdAsStringWithoutPrefix(), new UnspentTransaction { Value = transactionAmount, Hash = transactionHash, Index = index.Value });

            return (transaction.Id.IdAsStringWithoutPrefix(), transactionHash);
        }

        private async Task<(string, string)> CreateDebitTransactionAsync(string userId, string addressId, string destinationAddress, decimal transactionAmount)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(userId));
            var address = await _addressRepository.GetByIdAsync(new AddressId(addressId));
            decimal transactionFee = 0.00m;
            if (address.GetCryptoCurrencyType().Type == CryptoCurrencyTypes.BTC)
            {
                var cacheValue = await _cacheStorage.RetrieveAsync("CURRENT_BITCOIN_TRANSACTION_FEE");
                //transactionFee = decimal.Parse(cacheValue);
                transactionFee = 0.00015m;

                //if (transactionFee < 0.00006m)
                //{
                //    transactionFee = 0.00006m;
                //}
            }
            else
            {
                //var cacheValue = await _cacheStorage.RetrieveAsync("CURRENT_ETHEREUM_GAS_PRICE");
                //transactionFee = decimal.Parse(cacheValue);
                transactionFee = 0.000021m; // standard eth transaction fees in eth
            }
            decimal totalToSpend = transactionAmount + transactionFee;
            if (!address.HasBalanceForTransaction(totalToSpend))
                throw new InsufficientBalanceException();
            var unspentTransactions = await _addressWriter.PopUnspentTransactions(addressId, totalToSpend);

            _logger.Debug($"UnspentTransactions to use: {JsonConvert.SerializeObject(unspentTransactions)}");

            if(unspentTransactions is object && unspentTransactions.Any())
            {
                foreach (var tx in unspentTransactions)
                {
                    // cache for change transaction.
                    await _cacheStorage.StoreAsync(tx.Hash, $"CHANGE_TRANSACTION:{totalToSpend}:{address.GetAddressString()}", DateTime.Now.AddDays(1), TimeSpan.FromDays(1));
                }
            }

            var transaction = new Domain.TransactionModule.Transaction(TransactionId.NewTransactionId(), address.Id, user.Id,
                CryptoCurrency.NewCryptoCurrency(transactionAmount), DestinationAddress.NewDestinationAddress(destinationAddress, 
                address.GetCryptoCurrencyType()), TransactionType.Debit, CryptoCurrency.NewCryptoCurrency(transactionFee), unspentTransactions.ToList());
            
            // broadcast transaction here....
            var transactionHash = await BlockchainProvider.InitializeFactories()
                .ExecuteCreation(address.GetCryptoCurrencyType().Type)
                .Broadcast(transaction, address);

            _logger.Debug($"{transactionHash}");
            
            if(transactionHash.Length < 64)
            {
                _logger.Debug($"Failed Transaction, add the following unspentTransactions {JsonConvert.SerializeObject(unspentTransactions)}");
                await _unSpentTransactionUpdater.UpdateUnSpentTransactionToUnSpent(unspentTransactions.ToList());
                await _addressWriter.AddUnUsedUnspentTransactions(addressId, unspentTransactions.ToList());
                return (String.Empty, String.Empty);
            }

            _subscriber.Subscribe<TransactionCreatedEvent>(async @event => await HandleAsync(_transactionCreatedEventHandlers, @event));
            
            await _transactionRepository.SaveAsync(transaction);

            await _addressWriter.UpdateBalanceAsync(address.Id.IdAsStringWithoutPrefix(), transaction.GetTotalTransactionAmount(), TransactionTypes.Debit);

            return (transaction.Id.IdAsStringWithoutPrefix(), transactionHash);
        }
        public async Task HandleAsync<T>(IEnumerable<IDomainEventHandler<TransactionId, T>> handlers, T @event)
            where T : IDomainEvent<TransactionId>
        {
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event);
            }
        }
    }
}
