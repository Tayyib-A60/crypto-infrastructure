using BitcoinLib.Responses.SharedComponents;

namespace WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents
{
    public class BitcoinTransactionReceivedIntegrationEvent : IntegrationEvent
    {
        public BitcoinTransactionReceivedIntegrationEvent(Vout vo, string txHash, int index, bool isChange = false, decimal bookBalanceDifference = 0.00m)
        {
            Vout = vo;
            TransactionHash = txHash;
            Index = index;
            IsChange = isChange;
            BookBalanceDifference = bookBalanceDifference;

        }

        public Vout Vout { get; set;  }
        public string TransactionHash { get; set; }
        public int Index;
        public bool IsChange { get; set; }
        public decimal BookBalanceDifference { get; set; }
    }
}
