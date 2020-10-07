using BitcoinLib.Responses.SharedComponents;
using System.Collections.Generic;

namespace WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents
{
    public class BitcoinTransactionReceivedIntegrationEvent : IntegrationEvent
    {
        public BitcoinTransactionReceivedIntegrationEvent(string unTxhash, Vout vo, Vout sender, string txHash, int index, bool isChange = false, decimal bookBalanceDifference = 0.00m)
        {
            Vout = vo;
            Sender = sender;
            TransactionHash = txHash;
            Index = index;
            IsChange = isChange;
            BookBalanceDifference = bookBalanceDifference;
            UnspentTxHash = unTxhash;

        }

        public Vout Vout { get; set;  }
        public Vout Sender { get; set; }
        public string UnspentTxHash { get; set; }
        public string TransactionHash { get; set; }
        public int Index;
        public bool IsChange { get; set; }
        public decimal BookBalanceDifference { get; set; }
    }
}
