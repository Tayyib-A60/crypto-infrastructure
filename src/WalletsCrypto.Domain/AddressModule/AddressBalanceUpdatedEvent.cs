using WalletsCrypto.Domain.Core;
using WalletsCrypto.Domain.SharedKernel;

namespace WalletsCrypto.Domain.AddressModule
{
    public class AddressBalanceUpdatedEvent : DomainEventBase<AddressId>
    {
        AddressBalanceUpdatedEvent()
        {
        }

        internal AddressBalanceUpdatedEvent(AddressId aggregateId, CryptoCurrency availableBalance, CryptoCurrency bookBalance)
            : base(aggregateId)
        {
            AvailableBalance = availableBalance;
            BookBalance = bookBalance;
        }

        private AddressBalanceUpdatedEvent(AddressId aggregateId, long aggregateVersion, CryptoCurrency availableBalance, CryptoCurrency bookBalance)
            : base(aggregateId, aggregateVersion)
        {
            AvailableBalance = availableBalance;
            BookBalance = bookBalance;
        }

        public CryptoCurrency AvailableBalance { get; private set; }
        public CryptoCurrency BookBalance { get; private set; }
        public override IDomainEvent<AddressId> WithAggregate(AddressId aggregateId, long aggregateVersion)
        {
            return new AddressBalanceUpdatedEvent(aggregateId, aggregateVersion, AvailableBalance, BookBalance);
        }
    }
}
