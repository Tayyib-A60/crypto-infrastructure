using Nethereum.RPC.Eth.DTOs;

namespace WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents
{
    public class EthereumTransactionReceivedIntegrationEvent : IntegrationEvent
    {
        public EthereumTransactionReceivedIntegrationEvent(Transaction tx, bool transactionConfirmation)
        {
            Transaction = tx;
            IsTransactionConfirmation = transactionConfirmation;
        }
        public Transaction Transaction { get; set; }
        public bool IsTransactionConfirmation { get; set; }
    }
}
