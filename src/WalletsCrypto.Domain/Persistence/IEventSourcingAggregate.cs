using System.Collections.Generic;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.Persistence
{
    internal interface IEventSourcingAggregate<TAggregateId>
    {
        long Version { get; }
        void ApplyEvent(IDomainEvent<TAggregateId> @event, long version);
        IEnumerable<IDomainEvent<TAggregateId>> GetUncommittedEvents();
        void ClearUncommittedEvents();
    }
}