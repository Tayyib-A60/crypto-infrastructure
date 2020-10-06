using System.Collections.Generic;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.AddressModule
{
    public class UnspentTransactionAddedEvent : DomainEventBase<AddressId>
    {
        UnspentTransactionAddedEvent()
        {
        }

        internal UnspentTransactionAddedEvent(AddressId aggregateId, List<UnspentTransaction> unspentTransactions)
            : base(aggregateId)
        {
            UnspentTransactions = unspentTransactions;
        }

        private UnspentTransactionAddedEvent(AddressId aggregateId, long aggregateVersion, List<UnspentTransaction> unspentTransactions)
            : base (aggregateId, aggregateVersion)
        {
            UnspentTransactions = unspentTransactions;
        }

        public List<UnspentTransaction> UnspentTransactions { get; private set; }
        public override IDomainEvent<AddressId> WithAggregate(AddressId aggregateId, long aggregateVersion)
        {
            return new UnspentTransactionAddedEvent(aggregateId, aggregateVersion, UnspentTransactions);
        }
    }
}
