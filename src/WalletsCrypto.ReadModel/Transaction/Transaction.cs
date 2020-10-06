using System.Text.Json.Serialization;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.ReadModel.Common;

namespace WalletsCrypto.ReadModel.Transaction
{
    public class Transaction : IReadEntity
    {
        [JsonPropertyName("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonPropertyName("user_id")]
        public string UserId
        {
            get;
            set;
        }

        [JsonPropertyName("address_id")]
        public string AddressId
        {
            get;
            set;
        }

        [JsonPropertyName("transaction_amount")]
        public decimal TransactionAmount
        {
            get;
            set;
        }

        [JsonPropertyName("transaction_fee")]
        public decimal TransactionFee
        {
            get;
            set;
        }

        [JsonPropertyName("transaction_address")]
        public string TransactionAddress
        {
            get;
            set;
        }

        [JsonPropertyName("transaction_type")]
        public TransactionTypes TransactionType
        {
            get;
            set;
        }
    }
}
