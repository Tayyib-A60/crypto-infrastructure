using System.Text.Json.Serialization;
using WalletsCrypto.ReadModel.Common;

namespace WalletsCrypto.ReadModel.UnspentTransaction
{
    public class UnspentTransaction : IReadEntity
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonIgnore]
        public string AddressId { get; set; }

        [JsonIgnore]
        public string TxHash { get; set; }

        [JsonIgnore]
        public decimal Amount { get; set; }

        [JsonIgnore]
        public bool IsSpent { get; set; } = false;

        
    }
}
