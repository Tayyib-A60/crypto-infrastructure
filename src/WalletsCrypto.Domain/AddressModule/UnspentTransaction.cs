namespace WalletsCrypto.Domain.AddressModule
{
    public class UnspentTransaction
    {
        public string Hash { get; set; }
        public decimal Value { get; set; }
        public int Index { get; set; }
    }
}
