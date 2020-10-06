namespace WalletsCrypto.Domain.SharedKernel
{
    public class CryptoCurrencyType
    {
        private CryptoCurrencyType()
        {
        }
        private CryptoCurrencyType(CryptoCurrencyTypes type)
        {
            Type = type;
        }

        public static CryptoCurrencyType BTC
            = new CryptoCurrencyType(CryptoCurrencyTypes.BTC);

        public static CryptoCurrencyType ETH
            = new CryptoCurrencyType(CryptoCurrencyTypes.ETH);

        public CryptoCurrencyTypes Type { get; private set; }

    }
}
