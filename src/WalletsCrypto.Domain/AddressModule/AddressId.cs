using System;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.AddressModule
{
    public class AddressId : IAggregateId
    {
        private const string IdAsStringPrefix = "Address-";

        public Guid Id { get; private set; }

        private AddressId(Guid id)
        {
            Id = id;
        }

        public AddressId(string id)
        {
            Id = Guid.Parse(id.StartsWith(IdAsStringPrefix) ? id.Substring(IdAsStringPrefix.Length) : id);
        }

        public override string ToString()
        {
            return IdAsString();
        }

        public override bool Equals(object obj)
        {
            return obj is AddressId && Equals(Id, ((AddressId)obj).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static AddressId NewAddressId()
        {
            return new AddressId(Guid.NewGuid());
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