namespace WalletsCrypto.Domain.TransactionModule
{
    public class CreditingAddress : TransactionAddress
    {
        
        private CreditingAddress(string addressString)
        {
            AddressString = addressString;
        }

        private CreditingAddress()
        {

        }

        public static CreditingAddress NewCreditingAddress(string addressString)
        {
            return new CreditingAddress(addressString);
        }

    }
}
