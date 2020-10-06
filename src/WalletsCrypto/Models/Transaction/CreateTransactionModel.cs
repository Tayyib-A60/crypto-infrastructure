using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WalletsCrypto.Models.Transaction
{
    public class CreateTransactionModel
    {
        [Required]
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [Required]
        [JsonPropertyName("address_id")]
        public string AddressId { get; set; }

        [Required]
        [JsonPropertyName("destination_address")]
        public string DestinationAddress { get; set; }

        [Required]
        [JsonPropertyName("transaction_amount")]
        public decimal TransactionAmount { get; set; }
    }
}
