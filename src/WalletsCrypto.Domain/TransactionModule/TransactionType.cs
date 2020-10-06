namespace WalletsCrypto.Domain.TransactionModule
{
    public sealed class TransactionType
    {
        private TransactionType()
        {

        }
        private TransactionType(TransactionTypes type)
        {
            Type = type;
        }

        public static TransactionType Cedit
            = new TransactionType(TransactionTypes.Credit);

        public static TransactionType Debit
            = new TransactionType(TransactionTypes.Debit);

        public TransactionTypes Type { get; }
    }
}
