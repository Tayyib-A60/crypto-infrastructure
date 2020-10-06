using System;

namespace WalletsCrypto.Domain.TransactionModule
{
    [Serializable]
    public class TransactionException : Exception
    {
        public TransactionException() { }
        public TransactionException(string message) : base(message) { }
        public TransactionException(string message, Exception inner) : base(message, inner) { }
        protected TransactionException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
