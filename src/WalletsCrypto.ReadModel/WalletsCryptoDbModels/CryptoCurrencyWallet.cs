using System;

namespace WalletsCrypto.ReadModel.WalletsCryptoDbModels
{
    public class CryptoCurrencyWallet
    {
        public long CryptoCurrencyWalletId { get; set; }
        public string WalletName { get; set; }

        public string UserPhoneNumber { get; set; }

        public string WalletUserId { get; set; }

        public string Address { get; set; }

        public string AddressId { get; set; }

        public CryptoCurrencyType CurrencyType { get; set; }

        public decimal AvailableBalance { get; set; }

        public decimal LedgerBalance { get; set; }

        public decimal DailyLimit { get; set; }

        public decimal MonthlyLimit { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public bool HasPendingUpdate { get; set; }
    }
}
