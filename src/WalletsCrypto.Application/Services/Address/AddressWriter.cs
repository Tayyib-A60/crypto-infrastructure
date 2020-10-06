using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLogWrapper;
using WalletsCrypto.Application.Handlers.UnSpentTransaction;
using WalletsCrypto.Application.Services.User;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.Core;
using WalletsCrypto.Domain.Persistence;
using WalletsCrypto.Domain.PubSub;
using WalletsCrypto.Domain.SharedKernel;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.Domain.UserModule;
using WalletsCrypto.ReadModel.Persistence;
using UnspentTx = WalletsCrypto.Domain.AddressModule.UnspentTransaction;
using UnspentTransactionReadModel = WalletsCrypto.ReadModel.UnspentTransaction.UnspentTransaction;


namespace WalletsCrypto.Application.Services.Address
{
    public class AddressWriter : IAddressWriter
    {
        private readonly IRepository<Domain.AddressModule.Address, AddressId> _addressRepository;
        private readonly ITransientDomainEventSubscriber _subscriber;
        private readonly IRepository<Domain.UserModule.User, UserId> _userRepository;
        private readonly IUserWriter _userWriter;
        private readonly IEnumerable<IDomainEventHandler<AddressId, AddressCreatedEvent>> _addressCreatedEventHandlers;
        private readonly IEnumerable<IDomainEventHandler<AddressId, AddressBalanceUpdatedEvent>> _addressBalanceUpdatedEventHandlers;
        private readonly IEnumerable<IDomainEventHandler<AddressId, UnspentTransactionAddedEvent>> _unspentTransactionAddedEventHandlers;
        private readonly IEnumerable<IDomainEventHandler<AddressId, UnspentTransactionRemovedEvent>> _unspentTransactionRemovedEventHandlers;
        private readonly IUnSpentTransactionUpdater _unSpentTransactionUpdater;
        private readonly IRepository<UnspentTransactionReadModel> _unspentTransactionRepository;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        public AddressWriter(
            IRepository<Domain.AddressModule.Address, AddressId> addressRepository,
            ITransientDomainEventSubscriber subscriber,
            IUserWriter userWriter,
            IRepository<Domain.UserModule.User, UserId> userRepository,
            IEnumerable<IDomainEventHandler<AddressId, AddressCreatedEvent>> addressCreatedEventHandlers,
            IEnumerable<IDomainEventHandler<AddressId, AddressBalanceUpdatedEvent>> addressBalanceUpdatedEventHandlers,
            IEnumerable<IDomainEventHandler<AddressId, UnspentTransactionAddedEvent>> unspentTransactionAddedEventHandlers,
            IEnumerable<IDomainEventHandler<AddressId, UnspentTransactionRemovedEvent>> unspentTransactionRemovedEventHandlers,
            IRepository<UnspentTransactionReadModel> unspentTransactionRepository
            )
        {
            _userWriter = userWriter;
            _subscriber = subscriber;
            _addressRepository = addressRepository;
            _userRepository = userRepository;
            _addressCreatedEventHandlers = addressCreatedEventHandlers;
            _addressBalanceUpdatedEventHandlers = addressBalanceUpdatedEventHandlers;
            _unspentTransactionAddedEventHandlers = unspentTransactionAddedEventHandlers;
            _unspentTransactionRemovedEventHandlers = unspentTransactionRemovedEventHandlers;
            _unspentTransactionRepository = unspentTransactionRepository;
            _unSpentTransactionUpdater = new UnSpentTransactionUpdater(_unspentTransactionRepository);
        }

        public async Task AddUnspentTransactionAsync(string addressId, UnspentTx unspentTx)
        {
            var address = await _addressRepository.GetByIdAsync(new AddressId(addressId));
            if (address is null)
                throw new ArgumentOutOfRangeException(nameof(addressId));
            address.AddUnspentTransaction(unspentTx);
            _subscriber.Subscribe<UnspentTransactionAddedEvent>(async @event => await HandleAsync(_unspentTransactionAddedEventHandlers, @event));
            await _addressRepository.SaveAsync(address);
        }

        public async Task AddChangeUnspentTransaction(string addressId, UnspentTx unspentTx)
        {
            var address = await _addressRepository.GetByIdAsync(new AddressId(addressId));
            if (address is null)
                throw new ArgumentOutOfRangeException(nameof(addressId));
            address.AddChangeUnspentTransaction(unspentTx);
            await _unSpentTransactionUpdater.AddUnSpentTransaction(addressId, unspentTx);
            await _addressRepository.SaveAsync(address);
        }

        public async Task<IEnumerable<UnspentTransaction>> PopUnspentTransactions(string addressId, decimal amount)
        {
            var address = await _addressRepository.GetByIdAsync(new AddressId(addressId));
            var unspentTransactions = address.GetUnspentTransactionsForCurrentTransaction(amount);
            _subscriber.Subscribe<UnspentTransactionRemovedEvent>(async @event => await HandleAsync(_unspentTransactionRemovedEventHandlers, @event));
            await _addressRepository.SaveAsync(address);
            return unspentTransactions;
        }

        public async Task AddUnUsedUnspentTransactions(string addressId, List<UnspentTx> unspentTransactionsToAdd)
        {
            var address = await _addressRepository.GetByIdAsync(new AddressId(addressId));
            address.AddUnUsedUnspentTransactions(unspentTransactionsToAdd);
            await _addressRepository.SaveAsync(address);
        }

        public async Task UpdateBalanceAsync(string addressId, decimal transactionValue, TransactionTypes transactionType, bool isConfirmed = false)
        {
            var address = await _addressRepository.GetByIdAsync(new AddressId(addressId));
            if (address is null)
                throw new ArgumentOutOfRangeException(nameof(addressId));
            address.UpdateAddressBalance(CryptoCurrency.NewCryptoCurrency(transactionValue), transactionType, isConfirmed);
            _subscriber.Subscribe<AddressBalanceUpdatedEvent>(async @event => await HandleAsync(_addressBalanceUpdatedEventHandlers, @event));
            await _addressRepository.SaveAsync(address);

        }
        public async Task<string> CreateAsync(string userId, CryptoCurrencyTypes cryptoCurrencyType)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(userId));
            if(user is null)
            {
                await _userWriter.CreateAsync(userId);
                user = await _userRepository.GetByIdAsync(new UserId(userId));
            }
            CryptoCurrencyType cryptoType = null;
            switch(cryptoCurrencyType)
            {
                case CryptoCurrencyTypes.BTC:
                    cryptoType = CryptoCurrencyType.BTC;
                    break;
                case CryptoCurrencyTypes.ETH:
                    cryptoType = CryptoCurrencyType.ETH;
                    break;
            }
            var addressKey = BlockchainAddressKey.NewKey(cryptoType);
            var blockchainAddress = BlockchainAddress.NewAddress(addressKey, cryptoType);
            var address = new Domain.AddressModule.Address(AddressId.NewAddressId(), user.Id, cryptoType, addressKey, blockchainAddress);
            _subscriber.Subscribe<AddressCreatedEvent>(async @event => await HandleAsync(_addressCreatedEventHandlers, @event));
            await _addressRepository.SaveAsync(address);
            return address.Id.IdAsStringWithoutPrefix();
        }

        public async Task HandleAsync<T>(IEnumerable<IDomainEventHandler<AddressId, T>> handlers, T @event)
            where T : IDomainEvent<AddressId>
        {
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event);
            }
        }
    }
}