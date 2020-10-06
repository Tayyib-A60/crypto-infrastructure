namespace WalletsCrypto.Infrastructure.Providers
{
    public class BitcoinBlockchainFactory : BlockchainProviderFactory
    {
        public override IBlockchainProvider Create() => new BitcoinBlockchainProvider();
    }
}
