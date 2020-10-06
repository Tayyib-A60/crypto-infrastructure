using System;
using System.Collections.Generic;
using System.Text;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Domain.AddressModule
{
    public class UnspentTransactionRemovedEvent : DomainEventBase<AddressId>
    {
        UnspentTransactionRemovedEvent()
        {
        }

        internal UnspentTransactionRemovedEvent(AddressId aggregateId, List<UnspentTransaction> unspentTransactions)
            : base(aggregateId)
        {
            UnspentTransactions = unspentTransactions;
        }

        private UnspentTransactionRemovedEvent(AddressId aggregateId, long aggregateVersion, List<UnspentTransaction> unspentTransactions)
           : base(aggregateId, aggregateVersion)
        {
            UnspentTransactions = unspentTransactions;
        }

        public List<UnspentTransaction> UnspentTransactions { get; private set; }

        public override IDomainEvent<AddressId> WithAggregate(AddressId aggregateId, long aggregateVersion)
        {
            return new UnspentTransactionRemovedEvent(aggregateId, aggregateVersion, UnspentTransactions);
        }
    }
}
