using NLogWrapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using WalletsCrypto.Application.Services;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.ReadModel.Persistence;
using AddressReadModel = WalletsCrypto.ReadModel.Address.Address;
using UnspentTransactionReadModel = WalletsCrypto.ReadModel.UnspentTransaction.UnspentTransaction;

namespace WalletsCrypto.Application.Handlers.Address
{
    public class AddressUpdater : IDomainEventHandler<AddressId, AddressCreatedEvent>,
        IDomainEventHandler<AddressId, AddressBalanceUpdatedEvent>,
        IDomainEventHandler<AddressId, UnspentTransactionAddedEvent>,
        IDomainEventHandler<AddressId, UnspentTransactionRemovedEvent>
    {
        private readonly IRepository<AddressReadModel> _addressRepository;
        private readonly IRepository<UnspentTransactionReadModel> _unspentTransactionRepository;
        private readonly ICacheStorage _cache;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IWalletsAddressUpdater _walletAddressUpdater { get; }

        public AddressUpdater(ICacheStorage cache,
            IRepository<AddressReadModel> addressRepository,
            IRepository<UnspentTransactionReadModel> unspentTransactionRepository,
            IWalletsAddressUpdater walletAddressUpdater)
        {
            _cache = cache;
            _addressRepository = addressRepository;
            _unspentTransactionRepository = unspentTransactionRepository;
            _walletAddressUpdater = walletAddressUpdater;
        }


        public async Task HandleAsync(AddressCreatedEvent @event)
        {
            await _addressRepository.InsertAsync(new AddressReadModel
            {
                Id = @event.AggregateId.IdAsStringWithoutPrefix(),
                CryptoCurrencyType = @event.CryptoCurrencyType.Type,
                UserId = @event.UserId.IdAsStringWithoutPrefix(),
                BlockchainAddress = @event.BlockchainAddress.AddressString.ToLower(),
                AvailableBalance = @event.AvailableBalance.Value + 0.00m, // adding this incase there's no decimal place to number
                BookBalance = @event.BookBalance.Value + 0.00m
            });

            await _cache.StoreAsync(@event.BlockchainAddress.AddressString.ToLower(), "address created");
        }

        public async Task HandleAsync(AddressBalanceUpdatedEvent @event)
        {
            var address = await _addressRepository.GetByIdAsync(@event.AggregateId.IdAsStringWithoutPrefix());
            address.AvailableBalance = @event.AvailableBalance.Value + 0.00m;
            address.BookBalance = @event.BookBalance.Value + 0.00m;
            await _addressRepository.UpdateAsync(address);
            _logger.Debug("Address updated in Crypto Db");
            await _walletAddressUpdater.UpdateAddressBalance(@event.AvailableBalance.Value + 0.00m, @event.BookBalance.Value + 0.00m, @event.AggregateId.IdAsStringWithoutPrefix());
            _logger.Debug("Address updated in WalletsCrypto Db");
        }

        public async Task HandleAsync(UnspentTransactionAddedEvent @event)
        {
            foreach (var untx in @event.UnspentTransactions)
            {
                var tx = await _unspentTransactionRepository.GetByQueryString($"SELECT * FROM UnspentTransactions WHERE TxHash = '{untx.Hash}'");

                if ((tx?.TxHash == untx.Hash && tx?.Amount != untx.Value) || tx?.TxHash == null)
                {
                    tx = new UnspentTransactionReadModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        AddressId = @event.AggregateId.IdAsStringWithoutPrefix(),
                        TxHash = untx.Hash,
                        Amount = untx.Value,
                        IsSpent = false
                    };

                    await _unspentTransactionRepository.InsertAsync(tx);
                }

            }
        }

        public async Task HandleAsync(UnspentTransactionRemovedEvent @event)
        {
            var txs = await _unspentTransactionRepository.GetAllByQueryString($"SELECT * FROM UnspentTransactions WHERE AddressId = '{@event.AggregateId.IdAsStringWithoutPrefix()}'");
            foreach (var tx in txs)
            {
                if(!@event.UnspentTransactions.Any(un => un.Hash == tx.TxHash))
                {
                    tx.IsSpent = true;
                    await _unspentTransactionRepository.UpdateAsync(tx);
                }
            }
        }
    }
}