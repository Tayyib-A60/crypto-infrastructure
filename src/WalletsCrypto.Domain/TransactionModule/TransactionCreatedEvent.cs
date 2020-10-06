using System.Collections.Generic;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.Core;
using WalletsCrypto.Domain.SharedKernel;
using WalletsCrypto.Domain.UserModule;

namespace WalletsCrypto.Domain.TransactionModule
{
    public class TransactionCreatedEvent : DomainEventBase<TransactionId>
    {
        TransactionCreatedEvent()
        {
        }

        internal TransactionCreatedEvent(TransactionId aggregateId, AddressId addressId, UserId userId, 
            CryptoCurrency transactionAmount, TransactionAddress transactionAddress, TransactionType transactionType,
            CryptoCurrency transactionFee, List<UnspentTransaction> bitcoinTxIns) 
            : base(aggregateId)
        {
            UserId = userId;
            AddressId = addressId;
            TransactionAmount = transactionAmount;
            TransactionAddress = transactionAddress;
            TransactionType = transactionType;
            TransactionFee = transactionFee;
            BitcoinTxIns = bitcoinTxIns;
        }

        private TransactionCreatedEvent(TransactionId aggregateId, long aggregateVersion,
            AddressId addressId, UserId userId,
            CryptoCurrency transactionAmount, TransactionAddress transactionAddress, TransactionType transactionType,
            CryptoCurrency transactionFee, List<UnspentTransaction> bitcoinTxIns)
            : base(aggregateId, aggregateVersion)
        {
            UserId = userId;
            AddressId = addressId;
            TransactionAmount = transactionAmount;
            TransactionAddress = transactionAddress;
            TransactionType = transactionType;
            TransactionFee = transactionFee;
            BitcoinTxIns = bitcoinTxIns;
        }

        public UserId UserId { get; private set; }
        public AddressId AddressId { get; private set; }
        public CryptoCurrency TransactionAmount { get; private set; }
        public CryptoCurrency TransactionFee { get; private set; }
        public TransactionAddress TransactionAddress { get; private set; }
        public TransactionType TransactionType { get; private set; }
        public List<UnspentTransaction> BitcoinTxIns { get; set; }

        public override IDomainEvent<TransactionId> WithAggregate(TransactionId aggregateId, long aggregateVersion)
        {
            return new TransactionCreatedEvent(aggregateId, aggregateVersion, AddressId, UserId,
                TransactionAmount, TransactionAddress, TransactionType, TransactionFee, BitcoinTxIns);
        }
    }
}
