using WalletsCrypto.Common.Configuration;

namespace WalletsCrypto.Infrastructure.Providers
{
    public class EthereumBlockchainFactory : BlockchainProviderFactory
    {
        public override IBlockchainProvider Create() => new EthereumBlockckainProvider();
    }
}
