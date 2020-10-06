using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLogWrapper;
using WalletsCrypto.Domain.Core;
using WalletsCrypto.Domain.SharedKernel;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.Domain.UserModule;

namespace WalletsCrypto.Domain.AddressModule
{
    public class Address : AggregateBase<AddressId>
    {
        private Address()
        {
        }
       
        private UserId UserId { get; set; }
        private CryptoCurrencyType CryptoCurrencyType { get; set; }
        private BlockchainAddress BlockchainAddress { get; set; }
        private BlockchainAddressKey BlockchainAddressKey { get; set; }
        private CryptoCurrency BookBalance { get; set; }
        private CryptoCurrency AvailableBalance { get; set; }
        private List<UnspentTransaction> UnspentTransactions { get; set; }
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public Address(AddressId addressId, UserId userId, 
            CryptoCurrencyType cryptoCurrencyType,
            BlockchainAddressKey blockchainAddressKey,
            BlockchainAddress blockchainAddress
            ) 
            : this()
        {
            if (addressId is null)
            {
                throw new ArgumentNullException(nameof(addressId));
            }
            if (userId is null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            if(cryptoCurrencyType is null)
            {
                throw new ArgumentNullException(nameof(cryptoCurrencyType));
            }
            if(blockchainAddressKey is null)
            {
                throw new ArgumentNullException(nameof(blockchainAddressKey));
            }
            if(blockchainAddress is null)
            {
                throw new ArgumentNullException(nameof(blockchainAddress));
            }
            UnspentTransactions = new List<UnspentTransaction>();
            BookBalance = CryptoCurrency.NewCryptoCurrency(0.00m);
            AvailableBalance = CryptoCurrency.NewCryptoCurrency(0.00m);
            RaiseEvent(new AddressCreatedEvent(addressId, userId, cryptoCurrencyType, blockchainAddressKey, blockchainAddress, AvailableBalance, BookBalance, UnspentTransactions));
        }


        public string GetPrivateKeyString() => BlockchainAddressKey.PrivateKeyString;
        public CryptoCurrencyType GetCryptoCurrencyType() => CryptoCurrencyType;
        public CryptoCurrency GetAvailableBalance() => AvailableBalance;
        public string GetAddressString() => BlockchainAddress.AddressString;

        public List<UnspentTransaction> GetUnspentTransactionsForCurrentTransaction(decimal transactionAmountAndTransactionFee)
        {
            if (CryptoCurrencyType.Type == CryptoCurrencyTypes.ETH) return null;
            var sorted = UnspentTransactions.OrderBy(us => us.Value);
            var transactions = new List<UnspentTransaction>();
            var sum = 0.0m;
            foreach (var utx in sorted)
            {
                sum += utx.Value;
                transactions.Add(utx);
                UnspentTransactions.Remove(utx);
                if (sum >= transactionAmountAndTransactionFee) break;
            }
            _logger.Debug($"The Transaction will use the following UnTxs {JsonConvert.SerializeObject(transactions)}");

            _logger.Debug($"Rem UnspentTransactions are {JsonConvert.SerializeObject(UnspentTransactions)}");

            RaiseEvent(new UnspentTransactionRemovedEvent(Id, UnspentTransactions));
            return transactions;
        }

        public bool HasBalanceForTransaction(decimal transactionAmountAndTransactionFee)
        {
            return CryptoCurrencyType.Type switch
            {
                CryptoCurrencyTypes.BTC => HasBalanceForBitcoinTransaction(transactionAmountAndTransactionFee),
                CryptoCurrencyTypes.ETH => HasBalanceForEthereumTransaction(transactionAmountAndTransactionFee),
                _ => false,
            };
        }

        private bool HasBalanceForEthereumTransaction(decimal transactionAmountAndTransactionFee)
        {
            return AvailableBalance.Value > transactionAmountAndTransactionFee;
        }

        private bool HasBalanceForBitcoinTransaction(decimal transactionAmountAndTransactionFee)
        {
            decimal unspectTransactionsValueSum = UnspentTransactions.Sum(un => un.Value);
            _logger.Debug($"Total transaction amount{transactionAmountAndTransactionFee}, totalUnspentTxValue {unspectTransactionsValueSum}, Available balance {AvailableBalance.Value}");
            return unspectTransactionsValueSum > transactionAmountAndTransactionFee
                            && AvailableBalance.Value > transactionAmountAndTransactionFee;
        }

        public void AddUnspentTransaction(UnspentTransaction unspentTransaction)
        {
            UnspentTransactions.Add(unspentTransaction);
            RaiseEvent(new UnspentTransactionAddedEvent(Id, UnspentTransactions));
        }

        public void AddChangeUnspentTransaction(UnspentTransaction unspentTransaction)
        {
            UnspentTransactions.Add(unspentTransaction);
            _logger.Debug($"UnspentTransactions after adding change unTx are {JsonConvert.SerializeObject(UnspentTransactions)}");
        }

        public void AddUnUsedUnspentTransactions(List<UnspentTransaction> unspentTransactions)
        {
            UnspentTransactions.AddRange(unspentTransactions);
        }

        public void UpdateAddressBalance(CryptoCurrency transactionValue, TransactionTypes transactionType, bool confirmed = false)
        {
            switch (transactionType)
            {
                case TransactionTypes.Credit:
                    AvailableBalance += transactionValue;
                    BookBalance += transactionValue;
                    break;
                case TransactionTypes.Debit:
                    if(confirmed)
                    {
                        BookBalance -= transactionValue;
                    }
                    else
                    {
                        AvailableBalance -= transactionValue;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transactionType));
            }

            RaiseEvent(new AddressBalanceUpdatedEvent(Id, AvailableBalance, BookBalance));


        }


        public override string ToString()
        {
            return $"{{ Id: \"{Id}\", " +
                $"UserId: \"{UserId.IdAsString()}\", " +
                $"CryptoCurrencyType: \"{CryptoCurrencyType.Type}\"," +
                $"BlockchainAddressKey: \"{BlockchainAddressKey}\"," +
                $"BlockchainAddress: \"{BlockchainAddress.AddressString}\"," +
                $"AvailableBalance: \"{AvailableBalance.Value}\"," +
                $"UnspentTransactions: \"{UnspentTransactions}\"," +
                $"BookBalance: \"{BookBalance.Value}\"}}";
        }

        internal void Apply(AddressCreatedEvent ev)
        {
            Id = ev.AggregateId;
            UserId = ev.UserId;
            CryptoCurrencyType = ev.CryptoCurrencyType;
            BlockchainAddressKey = ev.BlockchainAddressKey;
            BlockchainAddress = ev.BlockchainAddress;
            AvailableBalance = ev.AvailableBalance;
            BookBalance = ev.BookBalance;
            UnspentTransactions = ev.Transactions;
        }

        internal void Apply(UnspentTransactionAddedEvent ev)
        {
            Id = ev.AggregateId;
            UnspentTransactions = ev.UnspentTransactions;

        }

        internal void Apply(UnspentTransactionRemovedEvent ev)
        {
            Id = ev.AggregateId;
            UnspentTransactions = ev.UnspentTransactions;
        }

        internal void Apply(AddressBalanceUpdatedEvent ev)
        {
            Id = ev.AggregateId;
            BookBalance = ev.BookBalance;
            AvailableBalance = ev.AvailableBalance;
        }


    }
}
