using System;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.UserModule
{
    public class User : AggregateBase<UserId>
    {
        private User()
        {

        }

        public User(UserId userId) : this()
        {
            if(userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            RaiseEvent(new UserCreatedEvent(userId));
        }

        public override string ToString()
        {
            return $"{{ Id: \"{Id}\"}}";
        }

        internal void Apply(UserCreatedEvent ev)
        {
            Id = ev.AggregateId;
        }
    }
}