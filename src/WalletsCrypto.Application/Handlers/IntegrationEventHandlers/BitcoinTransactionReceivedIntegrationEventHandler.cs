using Newtonsoft.Json;
using NLogWrapper;
using System.Threading.Tasks;
using WalletsCrypto.Application.Services.Address;
using WalletsCrypto.Application.Services.Transaction;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions;
using WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents;
using WalletsCrypto.Application.Handlers.Address;
using WalletsCrypto.ReadModel.WalletsCryptoDbModels;
using System;

namespace WalletsCrypto.Application.Handlers.IntegrationEventHandlers
{
    public class BitcoinTransactionReceivedIntegrationEventHandler : IIntegrationEventHandler<BitcoinTransactionReceivedIntegrationEvent>
    {
        private readonly ITransactionWriter _transactionWriter;
        private readonly IAddressReader _addressReader;
        private readonly IAddressWriter _addressWriter;
        private readonly ICacheStorage _cache;
        private readonly IWalletsAddressUpdater _walletAddressUpdater;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public BitcoinTransactionReceivedIntegrationEventHandler(
            ITransactionWriter transactionWriter,
            IAddressReader addressReader,
            IAddressWriter addressWriter,
            ICacheStorage cache,
            IWalletsAddressUpdater walletAddressUpdater
            )
        {
            _transactionWriter = transactionWriter;
            _addressReader = addressReader;
            _addressWriter = addressWriter;
            _cache = cache;
            _walletAddressUpdater = walletAddressUpdater;
        }
        public async Task Handle(BitcoinTransactionReceivedIntegrationEvent @event)
        {
            decimal amount = @event.Vout.Value;
            int index = @event.Index;
            var transactionHash = @event.TransactionHash;
            var bookBalanceDifference = @event.BookBalanceDifference;
            var senderAddress = new ReadModel.Address.Address();


            if (@event.Sender != null)
            {
                senderAddress = await _addressReader.GetByBlockchainAddress(@event.Sender.ScriptPubKey.Addresses[0].ToLower());
            }
            var address = await _addressReader.GetByBlockchainAddress(@event.Vout.ScriptPubKey.Addresses[0].ToLower());

            _logger.Debug($"{JsonConvert.SerializeObject(@event)}");

            if (@event.IsChange)
            {
                // need a way to get a hold of the owning transaction... 
                await _addressWriter.UpdateBalanceAsync(address.Id, bookBalanceDifference, TransactionTypes.Debit, true);
                await _addressWriter.AddUnspentTransactionAsync(address.Id, new UnspentTransaction { Hash = transactionHash, Index = index, Value = amount });
                await _cache.RemoveAsync(@event.UnspentTxHash);
                await _walletAddressUpdater.UpdateTransactionStatus(transactionHash);
                _logger.Debug("Unspent TX Added");
            }
            else
            {
                if (string.IsNullOrEmpty(senderAddress?.BlockchainAddress))
                {
                    var cryptoWallet = await _walletAddressUpdater.GetCryptoWallet(address.Id);

                    var btcExchangeRate = await _cache.RetrieveAsync("BITCOIN_USD_RATE");

                    if (!string.IsNullOrEmpty(cryptoWallet?.Address))
                    {
                        var created = await _walletAddressUpdater.CreateCreditTransaction(new WalletCryptoTransaction
                        {
                            Amount = amount + 0.0m,
                            AmountInDollars = Decimal.Parse(btcExchangeRate) * amount,
                            AmountInNaira = 0.0m,
                            DestinationAddress = cryptoWallet.Address,
                            DestinationAddressId = cryptoWallet.AddressId,
                            SourceAddressId = senderAddress.BlockchainAddress,
                            Category = "BTC Wallet Transfer",
                            CurrencyType = CryptoCurrencyType.Bitcoin,
                            CurrentBalance = cryptoWallet.AvailableBalance + amount + 0.00m,
                            PreviousBalance = cryptoWallet.AvailableBalance + 0.0m,
                            DateCreated = DateTime.Now,
                            FinalStatusTimeStamp = DateTime.Now,
                            Status = "COMPLETED",
                            FinalStatus = "SUCCESSFUL",
                            Narration = "Credit from External Wallet",
                            TransactionType = "Credit",
                            TransactionChannel = "Bitcoin Node",
                            TransactionReference = "",
                            WalletUserId = cryptoWallet.WalletUserId,
                            TransactionFee = 0.0m,
                            TransactionHash = transactionHash
                        });

                        if (!created)
                        {
                            _logger.Debug($"Failed to create credit transaction for User with UserAddress:{cryptoWallet.Address} Amount:{amount}");
                        }
                    }

                }

                var (id, txHash) = await _transactionWriter.CreateAsync(address.UserId, address.Id, "", amount, TransactionTypes.Credit, index, transactionHash);

                _logger.Debug($"TransactionId: {id}, TransactionHash: {txHash}");
            }
        }
    }
}
