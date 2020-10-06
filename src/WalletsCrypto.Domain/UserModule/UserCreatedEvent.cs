using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.UserModule
{
    public class UserCreatedEvent : DomainEventBase<UserId>
    {
        UserCreatedEvent()
        {
        }

        internal UserCreatedEvent(UserId aggregateId) : base(aggregateId)
        {
        }

        private UserCreatedEvent(UserId aggregateId, long aggregateVersion) 
            : base(aggregateId, aggregateVersion)
        {

        }

        public override IDomainEvent<UserId> WithAggregate(UserId aggregateId, long aggregateVersion)
        {
            return new UserCreatedEvent(aggregateId, aggregateVersion);
        }
        
    }
}