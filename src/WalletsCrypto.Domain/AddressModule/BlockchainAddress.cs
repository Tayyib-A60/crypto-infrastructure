using NBitcoin;
using Nethereum.Signer;
using System;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Domain.SharedKernel;

namespace WalletsCrypto.Domain.AddressModule
{
    public class BlockchainAddress
    {
        public string AddressString { get; private set; }

        private BlockchainAddress(string addressString)
        {
            AddressString = addressString;
        }

        private BlockchainAddress()
        {

        }
        public static BlockchainAddress NewAddress(BlockchainAddressKey blockchainAddressKey, 
            CryptoCurrencyType cryptoCurrencyType)
        {
            return cryptoCurrencyType.Type switch
            {
                CryptoCurrencyTypes.BTC => CreateBitcoinAddress(blockchainAddressKey),
                CryptoCurrencyTypes.ETH => CreateEthereumAddress(blockchainAddressKey),
                _ => throw new ArgumentOutOfRangeException(nameof(cryptoCurrencyType)),
            };
        }

        private static BlockchainAddress CreateEthereumAddress(BlockchainAddressKey key)
        {
            var ethSecret = new EthECKey(key.PrivateKeyString);
            var account = new Nethereum.Web3.Accounts.Account(ethSecret);
            return new BlockchainAddress(account.Address);
        }
        private static BlockchainAddress CreateBitcoinAddress(BlockchainAddressKey key)
        {
            var bitcoinSecret = new BitcoinSecret(key.PrivateKeyString);
            return new BlockchainAddress(bitcoinSecret.PubKey.GetAddress(ScriptPubKeyType.Segwit, ApplicationConfiguration.BitcoinNodeConfiguration.Network).ToString());
        }

    }
}
