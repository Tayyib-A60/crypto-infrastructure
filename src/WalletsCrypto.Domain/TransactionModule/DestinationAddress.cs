using NBitcoin;
using System;
using WalletsCrypto.Domain.SharedKernel;
using Nethereum.Util;
using WalletsCrypto.Common.Configuration;

namespace WalletsCrypto.Domain.TransactionModule
{
    public class DestinationAddress : TransactionAddress
    {
        private DestinationAddress(string addressString)
        {
            AddressString = addressString;
        }

        private DestinationAddress()
        {

        }

        public static DestinationAddress NewDestinationAddress(string addressString, CryptoCurrencyType cryptoCurrencyType)
        {
            if (string.IsNullOrWhiteSpace(addressString)) throw new ArgumentNullException(nameof(addressString));
            return cryptoCurrencyType.Type switch
            {
                CryptoCurrencyTypes.BTC => CreateBitcoinDestinationAddress(addressString),
                CryptoCurrencyTypes.ETH => CreateEthereumDestinationAddress(addressString),
                _ => throw new ArgumentOutOfRangeException(nameof(cryptoCurrencyType))
            };
        }

        private static DestinationAddress CreateEthereumDestinationAddress(string addressString)
        {
            if (addressString.IsValidEthereumAddressHexFormat())
                return new DestinationAddress(addressString);
            throw new TransactionException(nameof(addressString));
        }

        private static DestinationAddress CreateBitcoinDestinationAddress(string addressString)
        {
            try
            {
                // will throw exception if invalid address
                _ = BitcoinAddress.Create(addressString, ApplicationConfiguration.BitcoinNodeConfiguration.Network);
            }
            catch (FormatException ex)
            {
                throw new TransactionException(nameof(addressString), ex);
            }
            
            return new DestinationAddress(addressString);
        }
    }

}
