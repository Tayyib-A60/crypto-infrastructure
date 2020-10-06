using System.Text.Json.Serialization;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.SharedKernel;
using WalletsCrypto.ReadModel.Common;

namespace WalletsCrypto.ReadModel.Address
{
    public class Address : IReadEntity
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

        [JsonPropertyName("cryptocurrency_type")]
        public CryptoCurrencyTypes CryptoCurrencyType 
        { 
            get; 
            set; 
        }
        

        [JsonPropertyName("blockchain_address")]
        public string BlockchainAddress
        {
            get;
            set;
        }

        [JsonPropertyName("available_balance")]
        public decimal AvailableBalance
        {
            get;
            set;
        }

        [JsonPropertyName("book_balance")]
        public decimal BookBalance
        {
            get;
            set;
        }

    }
}