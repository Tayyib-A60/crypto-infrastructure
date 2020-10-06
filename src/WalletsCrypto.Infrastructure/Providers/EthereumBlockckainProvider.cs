using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.NonceServices;
using Nethereum.Web3;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Domain.AddressModule;
using Transaction = WalletsCrypto.Domain.TransactionModule.Transaction;

namespace WalletsCrypto.Infrastructure.Providers
{
    public class EthereumBlockckainProvider : IBlockchainProvider
    {

        public async Task<string> Broadcast(Transaction transaction, Address address)
        {
            try
            {
                var account = new Nethereum.Web3.Accounts.Account(address.GetPrivateKeyString());
                var web3 = new Web3(account, "https://ropsten.infura.io/v3/1d2c0135fee94855abc1d0cf49cd4edf");
                account.NonceService = new InMemoryNonceService(account.Address, web3.Client);

                //nonce 
                var currentNonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address, BlockParameter.CreatePending());
                var futureNonce = await account.NonceService.GetNextNonceAsync();


                // get gas price
                //var gasPriceResult = await web3.Eth.GasPrice.SendRequestAsync();
                //var gasEstimate = await web3.Eth.GetEtherTransferService().EstimateGasAsync(transaction.GetTransactionAddressString(), transaction.GetTransactionAmount());
                //var gasPrice = Web3.Convert.FromWei(gasPriceResult, Nethereum.Util.UnitConversion.EthUnit.Gwei);

                var transactionAmount = Web3.Convert.ToWei(transaction.GetTransactionAmount(), Nethereum.Util.UnitConversion.EthUnit.Ether);
                var transactionFee = transaction.GetTransactionFee() * 1_000_000_000m;
                var transactionInput = new TransactionInput
                {
                    From = account.Address,
                    To = transaction.GetTransactionAddressString(),
                    Gas = new HexBigInteger(new BigInteger(transactionFee)),
                    Value = new HexBigInteger(transactionAmount),
                };

                transactionInput.Nonce = futureNonce;


                //send transaction
                var trans = await web3.TransactionManager.SendTransactionAsync(transactionInput);

                return trans;
                
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            
            return String.Empty;

        }

        public decimal GetFee()
        {
            return 0.000021m;
        }
    }
}
