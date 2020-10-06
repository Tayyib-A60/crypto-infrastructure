using System.Collections.Generic;
using WalletsCrypto.Domain.SharedKernel;

namespace WalletsCrypto.Infrastructure.Providers
{
    public class BlockchainProvider
    {
        
        private readonly Dictionary<CryptoCurrencyTypes, BlockchainProviderFactory> _factories;

        private BlockchainProvider()
        {
            _factories = new Dictionary<CryptoCurrencyTypes, BlockchainProviderFactory>
            {
                { CryptoCurrencyTypes.BTC, new BitcoinBlockchainFactory() },
                { CryptoCurrencyTypes.ETH, new EthereumBlockchainFactory() }
            };
        }
        public static BlockchainProvider InitializeFactories() => new BlockchainProvider();

        public IBlockchainProvider ExecuteCreation(CryptoCurrencyTypes cryptoCurrencyType) => _factories[cryptoCurrencyType].Create();

    }
}
