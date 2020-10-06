using NBitcoin;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using System;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Domain.SharedKernel;

namespace WalletsCrypto.Domain.AddressModule
{
    public class BlockchainAddressKey
    {
        public string PrivateKeyString { get; private set; }
        public string PublicKeyString { get; private set; }

        private BlockchainAddressKey()
        {

        }

        private BlockchainAddressKey(string publicKeyString, string privateKeyString)
        {
            PublicKeyString = publicKeyString;
            PrivateKeyString = privateKeyString;
        }

        public static BlockchainAddressKey NewKey(CryptoCurrencyType cryptoCurrencyType)
        {
            return cryptoCurrencyType.Type switch
            {
                CryptoCurrencyTypes.BTC => CreateBitCoinAddressKey(),
                CryptoCurrencyTypes.ETH => CreateEthereumAddressKey(),
                _ => throw new ArgumentOutOfRangeException(nameof(cryptoCurrencyType)),
            };
        }

        public override string ToString()
        {
            return $"{{ PublicKey: \"{PublicKeyString}\", PrivateKey: \"{PrivateKeyString}\" }}";
        }

        private static BlockchainAddressKey CreateEthereumAddressKey()
        {
            var key = EthECKey.GenerateKey();
            return new BlockchainAddressKey(key.GetPubKey().ToHex(), key.GetPrivateKeyAsBytes().ToHex());
            
        }

        private static BlockchainAddressKey CreateBitCoinAddressKey()
        {
            var key = new Key();
            var privKey = key.GetBitcoinSecret(ApplicationConfiguration.BitcoinNodeConfiguration.Network);
            return new BlockchainAddressKey(privKey.PubKey.ToString(), privKey.ToString());
        }
    }
}
