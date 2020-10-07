using BitcoinLib.Requests.CreateRawTransaction;
using BitcoinLib.Requests.SignRawTransaction;
using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Services.Coins.Bitcoin;
using NBitcoin;
using NLogWrapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Domain.AddressModule;
using Transaction = WalletsCrypto.Domain.TransactionModule.Transaction;



namespace WalletsCrypto.Infrastructure.Providers
{
    public class BitcoinBlockchainProvider : IBlockchainProvider
    {
        private readonly ICoinService _coinService;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public BitcoinBlockchainProvider()
        {
            _coinService = new BitcoinService(ApplicationConfiguration.BitcoinNodeConfiguration.RPCUrl,
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCUsername,
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCPassword,
                ApplicationConfiguration.BitcoinNodeConfiguration.WalletPassword,
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCRequestTimeout);
        }
        public Task<string> Broadcast(Transaction transaction, Address address)
        {
            _logger.Debug("Begin broadcast");
            var change = transaction.GetBitcoinTxIns().Sum(utx => utx.Value) - transaction.GetTotalTransactionAmount();

            _logger.Debug("Got change");

            var rawTransactionRequest = new CreateRawTransactionRequest();

            foreach (var utx in transaction.GetBitcoinTxIns())
            {
                rawTransactionRequest.AddInput(new CreateRawTransactionInput
                {
                    TxId = utx.Hash,
                    Vout = utx.Index, // index of output to spend.
                });
            }

            _logger.Debug("Added Txins");

            rawTransactionRequest.AddOutput(new CreateRawTransactionOutput
            {
                Address = transaction.GetTransactionAddressString(),
                Amount = transaction.GetTransactionAmount(),
            });

            _logger.Debug("Added Txout");
            if (change > 0)
            {
                rawTransactionRequest.AddOutput(new CreateRawTransactionOutput
                {
                    Address = address.GetAddressString(),
                    Amount = change
                });
                _logger.Debug("Added Txout if change");
            }

            var rawTransactionHex = _coinService.CreateRawTransaction(rawTransactionRequest);
            var privateKey = address.GetPrivateKeyString();

            _logger.Debug("Got Private key");

            var signedRawTransactionWithPrivateKeyRequest = new SignRawTransactionWithKeyRequest(rawTransactionHex);
            signedRawTransactionWithPrivateKeyRequest.AddKey(privateKey);


            var signedRawTransactionHex = _coinService.SignRawTransactionWithKey(signedRawTransactionWithPrivateKeyRequest);

            _logger.Debug("Signed trxn wt Private key");
            
            try
            {
                _logger.Debug("About to send transaction to Network");
                var transactionHash = _coinService.SendRawTransaction(signedRawTransactionHex.Hex, 0);
                return Task.FromResult(transactionHash);
            }
            catch(Exception ex)
            {
                _logger.Debug(ex.ToString());
            }

            return Task.FromResult(String.Empty);
            
        }

        public decimal GetFee()
        {
            var feeResponse =  _coinService.EstimateSmartFee(1008);
            var fee = decimal.Parse(feeResponse.FeeRate.ToString());
            return fee;

        }
    }
}
