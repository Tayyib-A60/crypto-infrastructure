using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Threading;
using System.Threading.Tasks;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Ethereum.Watcher.Channels;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions;
using WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents;

namespace WalletsCrypto.Ethereum.Watcher.BackgroundServices
{
    public class ProcessTransactionsService : BackgroundService
    {
        private readonly Web3 _web3;
        private readonly IEventBus _eventBus;
        private readonly ICacheStorage _cache;
        private readonly ILogger<ProcessTransactionsService> _logger;
        private readonly BlockNumberTransferChannel _transactionsTransferChannel;


        public ProcessTransactionsService(
            ICacheStorage cache,
            IEventBus eventBus,
            BlockNumberTransferChannel transactionsTransferChannel,
            ILogger<ProcessTransactionsService> logger)
        {
            _cache = cache;
            _logger = logger;
            _eventBus = _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _web3 = new Web3(ApplicationConfiguration.EthereumNodeConfiguration.HttpsUrl);
            _transactionsTransferChannel = transactionsTransferChannel;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var blockNumber in _transactionsTransferChannel.ReadAllAsync())
            {
                try
                {
                    await Task.Delay(5000);
                    var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new BlockParameter(blockNumber));
                    if (block != null)
                    {
                        foreach (var transaction in block.Transactions)
                        {
                            string to = null;
                            string from = null;

                            if(!string.IsNullOrWhiteSpace(transaction.To))
                                to = await _cache.RetrieveAsync(transaction.To.ToLower());

                            if (!string.IsNullOrWhiteSpace(transaction.From))
                                from = await _cache.RetrieveAsync(transaction.From);

                            if (to != null) //  || from != null
                            {
                                PublishNewEthereumTransactionReceivedIntegrationEvent(transaction);
                            }
                            if(from != null)
                            {
                                PublishNewEthereumTransactionReceivedIntegrationEvent(transaction, true);
                            }

                        }
                    }
                    else
                    {
                        _logger.LogInformation("Query is null: " + blockNumber);
                        await _transactionsTransferChannel.AddBlockNumberAsync(blockNumber);
                        _logger.LogInformation("readded to queue: " + blockNumber);
                    }

                }
                catch(Exception e)
                {
                    _logger.LogInformation(e.Message);
                    await _transactionsTransferChannel.AddBlockNumberAsync(blockNumber);
                    _logger.LogInformation("readded to queue: " + blockNumber);
                }
                finally
                {
                }
            }
        }

        private void PublishNewEthereumTransactionReceivedIntegrationEvent(Transaction transaction, bool isTransactionConfirmation = false)
        {
            var @event = new EthereumTransactionReceivedIntegrationEvent(transaction, isTransactionConfirmation);
            _eventBus.Publish(@event);
        }
    }
}
