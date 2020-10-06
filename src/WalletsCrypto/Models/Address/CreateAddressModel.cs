using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.SharedKernel;

namespace WalletsCrypto.Models.Address
{
    public class CreateAddressModel
    {
        [Required]
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [Required]
        [JsonPropertyName("cryptocurrency_type")]
        public CryptoCurrencyTypes CryptoCurrencyType { get; set; }
    }
}