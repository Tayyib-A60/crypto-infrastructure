using Nethereum.Web3;
using System.Threading.Tasks;
using WalletsCrypto.Application.Services.Address;
using WalletsCrypto.Application.Services.Transaction;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions;
using WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents;

namespace WalletsCrypto.Application.Handlers.IntegrationEventHandlers
{
    public class EthereumTransactionReceivedIntegrationEventHandler : IIntegrationEventHandler<EthereumTransactionReceivedIntegrationEvent>
    {
        private readonly ITransactionWriter _transactionWriter;
        private readonly IAddressReader _addressReader;
        private readonly IAddressWriter _addressWriter;

        public EthereumTransactionReceivedIntegrationEventHandler(
            ITransactionWriter transactionWriter,
            IAddressReader addressReader,
            IAddressWriter addressWriter)
        {
            _transactionWriter = transactionWriter;
            _addressReader = addressReader;
            _addressWriter = addressWriter;
        }
        public async Task Handle(EthereumTransactionReceivedIntegrationEvent @event)
        {
            decimal amount = Web3.Convert.FromWei(@event.Transaction.Value.Value, Nethereum.Util.UnitConversion.EthUnit.Ether);
            

            if (@event.IsTransactionConfirmation)
            {
                var address = await _addressReader.GetByBlockchainAddress(@event.Transaction.From.ToLower());
                decimal gasPrice = 0.000021m;
                var bookBalanceDifference = gasPrice + amount;
                await _addressWriter.UpdateBalanceAsync(address.Id, bookBalanceDifference, TransactionTypes.Debit, true);
            }
            else
            {
                var address = await _addressReader.GetByBlockchainAddress(@event.Transaction.To.ToLower());
                await _transactionWriter.CreateAsync(address.UserId, address.Id, @event.Transaction.From, amount, TransactionTypes.Credit);
            }
           
        }
    }
}
