using System;

namespace WalletsCrypto.Domain.AddressModule
{
    [Serializable]
    public class AddressException : Exception
    {
        public AddressException() { }
        public AddressException(string message) : base(message) { }
        public AddressException(string message, Exception inner) : base(message, inner) { }
        protected AddressException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}