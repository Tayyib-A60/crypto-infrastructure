using System;
using System.Collections.Generic;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.Core;
using WalletsCrypto.Domain.SharedKernel;
using WalletsCrypto.Domain.UserModule;

namespace WalletsCrypto.Domain.TransactionModule
{
    public class Transaction : AggregateBase<TransactionId>
    {
        public Transaction()
        {
        }

        public string GetTransactionAddressString() => TransactionAddress.AddressString;
        public decimal GetTransactionAmount() => TransactionAmount.Value;
        public decimal GetTransactionFee() => TransactionFee.Value;
        public decimal GetTotalTransactionAmount() => GetTransactionAmount() + GetTransactionFee();
        public List<UnspentTransaction> GetBitcoinTxIns() => BitcoinTxIns;

        private AddressId AddressId { get; set; }
        private UserId UserId { get; set; }
        private TransactionType TransactionType { get; set; }
        private CryptoCurrency TransactionAmount { get; set; }
        private TransactionAddress TransactionAddress { get; set; }
        private CryptoCurrency TransactionFee { get; set; }
        private List<UnspentTransaction> BitcoinTxIns { get; set; } // applicable to only bitcoin

        public Transaction(TransactionId transactionId, AddressId addressId, UserId userId,
            CryptoCurrency transactionAmount, TransactionAddress transactionAddress, 
            TransactionType transactionType, CryptoCurrency transactionFee = null, 
            List<UnspentTransaction>  bitcointxIns = null) : this()
        {
            if(transactionId is null)
            {
                throw new ArgumentNullException(nameof(transactionId));
            }
            if(addressId is null)
            {
                throw new ArgumentNullException(nameof(addressId));
            }
            if(userId is null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            if(transactionAmount is null)
            {
                throw new ArgumentNullException(nameof(transactionAmount));
            }
            if(transactionAddress is null)
            {
                throw new ArgumentNullException(nameof(transactionAddress));
            }
            if(transactionType is null)
            {
                throw new ArgumentNullException(nameof(transactionType));
            }
            if(transactionType.Type == TransactionTypes.Debit && transactionFee == null)
            {
                throw new ArgumentNullException(nameof(transactionFee));
            }
            RaiseEvent(new TransactionCreatedEvent(transactionId, addressId, userId, transactionAmount,
                transactionAddress, transactionType, transactionFee, bitcointxIns));
        }

        
        public override string ToString()
        {
            return $"{{ Id: \"{Id}\", " +
                $"AddressId: \"{AddressId.IdAsString()}\", " +
                $"UserId: \"{UserId.IdAsString()}\", " +
                $"TransactionType: \"{TransactionType.Type}\"," +
                $"TransactionAmount: \"{TransactionAmount.Value}\"" +
                $"TransactionFee: \"{TransactionFee}\"" +
                $"BitcoinTxIns: \"{BitcoinTxIns}\"" +
                $"DestinationAddress: \"{TransactionAddress?.AddressString}\"}}";
        }

        internal void Apply(TransactionCreatedEvent ev)
        {
            Id = ev.AggregateId;
            AddressId = ev.AddressId;
            UserId = ev.UserId;
            TransactionType = ev.TransactionType;
            TransactionAmount = ev.TransactionAmount;
            TransactionAddress = ev.TransactionAddress;
            TransactionFee = ev.TransactionFee;
            BitcoinTxIns = ev.BitcoinTxIns;
        }
    }
}
