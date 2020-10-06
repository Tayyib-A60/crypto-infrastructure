using System;
using System.Collections.Generic;
using System.Text;

namespace WalletsCrypto.ReadModel.WalletsCryptoDbModels
{

    public class WalletCryptoTransaction
    {
        public string DestinationAddress { get; set; }
        public DateTime FinalStatusTimeStamp { get; set; }
        public string FinalStatus { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal PreviousBalance { get; set; }
        public string Narration { get; set; }
        public CryptoCurrencyType CurrencyType { get; set; }
        public decimal AmountInNaira { get; set; }
        public decimal AmountInDollars { get; set; }
        public decimal Amount { get; set; }
        public string TransactionChannel { get; set; }
        public string TransactionReference { get; set; }
        public string TransactionType { get; set; }
        public string DestinationAddressId { get; set; }
        public string SourceAddressId { get; set; }
        public string WalletUserId { get; set; }
        public long CryptoWalletTransactionId { get; set; }
        public string Category { get; set; }
        public decimal TransactionFee { get; set; }
    }

    public enum CryptoCurrencyType
    {
        Bitcoin = 0,
        Ethereum = 1,
        Invalid = 99
    }
}
