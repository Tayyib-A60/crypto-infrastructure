using System.Collections.Generic;
using WalletsCrypto.Domain.Core;
using WalletsCrypto.Domain.SharedKernel;
using WalletsCrypto.Domain.UserModule;

namespace WalletsCrypto.Domain.AddressModule
{
    public class AddressCreatedEvent : DomainEventBase<AddressId>
    {
        AddressCreatedEvent()
        {
        }

        internal AddressCreatedEvent(AddressId aggregateId, UserId userId, CryptoCurrencyType cryptoCurrencyType, 
            BlockchainAddressKey blockchainAddresskey, BlockchainAddress blockchainAddress, 
            CryptoCurrency availableBalance, CryptoCurrency bookBalance, List<UnspentTransaction> transactions) : base(aggregateId)
        {
            UserId = userId;
            CryptoCurrencyType = cryptoCurrencyType;
            BlockchainAddressKey = blockchainAddresskey;
            BlockchainAddress = blockchainAddress;
            AvailableBalance = availableBalance;
            BookBalance = bookBalance;
            Transactions = transactions;
        }

        private AddressCreatedEvent(AddressId aggregateId, long aggregateVersion,
            UserId userId, CryptoCurrencyType cryptoCurrencyType,
            BlockchainAddressKey blockchainAddressKey, BlockchainAddress blockchainAddress, CryptoCurrency availableBalance, CryptoCurrency bookBalance, List<UnspentTransaction> transactions) 
                : base(aggregateId, aggregateVersion)
        {
            UserId = userId;
            CryptoCurrencyType = cryptoCurrencyType;
            BlockchainAddressKey = blockchainAddressKey;
            BlockchainAddress = blockchainAddress;
            AvailableBalance = availableBalance;
            BookBalance = bookBalance;
            Transactions = transactions;
        }

        public UserId UserId { get; private set; }
        public CryptoCurrencyType CryptoCurrencyType { get; private set; }
        public BlockchainAddressKey BlockchainAddressKey { get; private set; }
        public BlockchainAddress BlockchainAddress { get; private set; }
        public CryptoCurrency AvailableBalance { get; private set; }
        public CryptoCurrency BookBalance { get; private set; }
        public List<UnspentTransaction> Transactions { get; private set; }

        public override IDomainEvent<AddressId> WithAggregate(AddressId aggregateId, long aggregateVersion)
        {
            return new AddressCreatedEvent(aggregateId, aggregateVersion, UserId, CryptoCurrencyType, BlockchainAddressKey, BlockchainAddress, AvailableBalance, BookBalance, Transactions);
        }
    }
}