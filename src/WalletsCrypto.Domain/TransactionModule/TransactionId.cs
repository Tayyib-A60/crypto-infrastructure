using System;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.TransactionModule
{
    public class TransactionId : IAggregateId
    {
        private const string IdAsStringPrefix = "Transaction-";

        public Guid Id { get; private set; }

        public TransactionId(Guid id)
        {
            Id = id;
        }
        public TransactionId(string id)
        {
            Id = Guid.Parse(id.StartsWith(IdAsStringPrefix) ? id.Substring(IdAsStringPrefix.Length) : id);
        }

        public override string ToString()
        {
            return IdAsString();
        }

        public override bool Equals(object obj)
        {
            return obj is TransactionId && Equals(Id, ((TransactionId)obj).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static TransactionId NewTransactionId()
        {
            return new TransactionId(Guid.NewGuid());
        }

        public string IdAsString()
        {
            return $"{IdAsStringPrefix}{Id}";
        }

        public string IdAsStringWithoutPrefix()
        {
            return Id.ToString();
        }
    }

}
