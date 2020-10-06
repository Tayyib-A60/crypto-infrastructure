using Newtonsoft.Json;
using NLogWrapper;
using System.Threading.Tasks;
using WalletsCrypto.Application.Services.Address;
using WalletsCrypto.Application.Services.Transaction;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions;
using WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents;

namespace WalletsCrypto.Application.Handlers.IntegrationEventHandlers
{
    public class BitcoinTransactionReceivedIntegrationEventHandler : IIntegrationEventHandler<BitcoinTransactionReceivedIntegrationEvent>
    {
        private readonly ITransactionWriter _transactionWriter;
        private readonly IAddressReader _addressReader;
        private readonly IAddressWriter _addressWriter;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public BitcoinTransactionReceivedIntegrationEventHandler(
            ITransactionWriter transactionWriter,
            IAddressReader addressReader,
            IAddressWriter addressWriter)
        {
            _transactionWriter = transactionWriter;
            _addressReader = addressReader;
            _addressWriter = addressWriter;
        }
        public async Task Handle(BitcoinTransactionReceivedIntegrationEvent @event)
        {
            decimal amount = @event.Vout.Value;
            int index = @event.Index;
            var transactionHash = @event.TransactionHash;
            var bookBalanceDifference = @event.BookBalanceDifference;

            var address = await _addressReader.GetByBlockchainAddress(@event.Vout.ScriptPubKey.Addresses[0].ToLower());

            _logger.Debug($"{JsonConvert.SerializeObject(@event)}");

            if (@event.IsChange)
            {
                // need a way to get a hold of the owning transaction... 
                await _addressWriter.UpdateBalanceAsync(address.Id, bookBalanceDifference, TransactionTypes.Debit, true);
                await _addressWriter.AddUnspentTransactionAsync(address.Id, new UnspentTransaction { Hash = transactionHash, Index = index, Value = amount });
                _logger.Debug("Unspent TX Added");
            }
            else
            {
                await _transactionWriter.CreateAsync(address.UserId, address.Id, "", amount, TransactionTypes.Credit, index, transactionHash);
            }
        }
    }
}
