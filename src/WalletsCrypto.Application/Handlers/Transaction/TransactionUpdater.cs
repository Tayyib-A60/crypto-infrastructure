using NLogWrapper;
using System;
using System.Threading.Tasks;
using WalletsCrypto.Application.Handlers.Address;
using WalletsCrypto.Application.Services;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.ReadModel.Persistence;
using WalletsCrypto.ReadModel.WalletsCryptoDbModels;
using TransactionReadModel = WalletsCrypto.ReadModel.Transaction.Transaction;

namespace WalletsCrypto.Application.Handlers.Transaction
{
    public class TransactionUpdater : IDomainEventHandler<TransactionId, TransactionCreatedEvent>
    {
        private readonly IRepository<TransactionReadModel> _transactionRepository;
        private IWalletsAddressUpdater _walletAddressUpdater { get; }
        private readonly ICacheStorage _cache;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public TransactionUpdater(IRepository<TransactionReadModel> transactionRepository,
            IWalletsAddressUpdater walletAddressUpdater, ICacheStorage cache)
        {
            _walletAddressUpdater = walletAddressUpdater;
            _transactionRepository = transactionRepository;
            _cache = cache;
        }

        public async Task HandleAsync(TransactionCreatedEvent @event)
        {
            await _transactionRepository.InsertAsync(new TransactionReadModel
            {
                Id = @event.AggregateId.IdAsStringWithoutPrefix(),
                UserId = @event.UserId.IdAsStringWithoutPrefix(),
                AddressId = @event.AddressId.IdAsStringWithoutPrefix(),
                TransactionAmount = @event.TransactionAmount.Value + 0.00m, // adding this incase there's no decimal place to number
                TransactionAddress = @event.TransactionAddress.AddressString,
                TransactionType = @event.TransactionType.Type,
                TransactionFee = @event.TransactionFee is object ? @event.TransactionFee.Value + 0.00m : 0.00m
            });

            //if(@event.TransactionType.Type == TransactionTypes.Credit)
            //{
            //    var cryptoWallet = await _walletAddressUpdater.GetCryptoWallet(@event.AddressId.IdAsStringWithoutPrefix());

            //    var btcExchangeRate = await _cache.RetrieveAsync("BITCOIN_USD_RATE");

            //    if(!String.IsNullOrEmpty(cryptoWallet.Address))
            //    {
            //        var created  = await _walletAddressUpdater.CreateCreditTransaction(new WalletCryptoTransaction
            //        {
            //            Amount = @event.TransactionAmount.Value + 0.0m,
            //            AmountInDollars = Decimal.Parse(btcExchangeRate) * @event.TransactionAmount.Value,
            //            AmountInNaira = 0.0m,
            //            DestinationAddress = cryptoWallet.Address,
            //            DestinationAddressId = cryptoWallet.AddressId,
            //            SourceAddressId = "",
            //            Category = "BTC Wallet Transfer",
            //            CurrencyType = CryptoCurrencyType.Bitcoin,
            //            CurrentBalance = cryptoWallet.AvailableBalance + @event.TransactionAmount.Value + 0.00m,
            //            PreviousBalance = cryptoWallet.AvailableBalance,
            //            DateCreated = DateTime.Now,
            //            FinalStatusTimeStamp = DateTime.Now,
            //            Status = "COMPLETED",
            //            FinalStatus = "SUCCESSFUL",
            //            Narration = "Credit from External Wallet",
            //            TransactionType = "Credit",
            //            TransactionChannel = "Bitcoin Node",
            //            TransactionReference = @event.AggregateId.IdAsStringWithoutPrefix(),
            //            WalletUserId = cryptoWallet.WalletUserId,
            //            TransactionFee = 0.0m
            //        });

            //        if(!created)
            //        {
            //            _logger.Debug($"Failed to create credit transaction for User with UserId:{@event.UserId.IdAsStringWithoutPrefix()} Amount:{@event.TransactionAmount.Value}");
            //        }
            //    }

            //}


        }

    }
}
