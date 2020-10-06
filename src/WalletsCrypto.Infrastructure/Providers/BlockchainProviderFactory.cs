
namespace WalletsCrypto.Infrastructure.Providers
{
    public abstract class BlockchainProviderFactory
    {
        public abstract IBlockchainProvider Create();
    }
}
