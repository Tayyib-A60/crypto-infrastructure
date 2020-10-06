using System;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.UserModule
{
    public class UserId : IAggregateId
    {
        private const string IdAsStringPrefix = "User-";

        public Guid Id { get; private set; }

        public UserId(string id)
        {
            Id = Guid.Parse(id.StartsWith(IdAsStringPrefix) ? id.Substring(IdAsStringPrefix.Length) : id);
        }

        public override string ToString()
        {
            return IdAsString();
        }

        public override bool Equals(object obj)
        {
            return obj is UserId id && Equals(Id, id.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
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