using BitcoinLib.Responses;
using BitcoinLib.Responses.SharedComponents;
using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Services.Coins.Bitcoin;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using WalletsCrypto.Bitcoin.Watcher.Channels;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions;
using WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents;
using System.Linq;
using System.Collections.Generic;
using NLogWrapper;
using Newtonsoft.Json;

namespace WalletsCrypto.Bitcoin.Watcher.BackgroundServices
{
    public class ProcessTransactionsService : BackgroundService
    {
        private readonly IEventBus _eventBus;
        private readonly ICacheStorage _cache;
        private readonly ICoinService _coinService;
        private readonly BlockHashTransferChannel _blockHasTransferChannel;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public ProcessTransactionsService(
            ICacheStorage cache,
            IEventBus eventBus,
            BlockHashTransferChannel blockHasTransferChannel
            )
        {
            _cache = cache;
            _eventBus = _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _blockHasTransferChannel = blockHasTransferChannel;
            _coinService = new BitcoinService(ApplicationConfiguration.BitcoinNodeConfiguration.RPCUrl, 
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCUsername, 
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCPassword, 
                ApplicationConfiguration.BitcoinNodeConfiguration.WalletPassword, 
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCRequestTimeout); 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var blockHash in _blockHasTransferChannel.ReadAllAsync())
            {
                try
                {
                    // await Task.Delay(5000);
                    var block = _coinService.GetBlock(blockHash, 2);


                    // need to handle for TIn



                    foreach (IncludedTransaction transaction in block.Tx)
                    {
                        
                        foreach (var outTx in transaction.Vout)
                        {
                            if (outTx.ScriptPubKey.Addresses is null) continue;
                            foreach (var address in outTx.ScriptPubKey.Addresses)
                            {
                                if (!string.IsNullOrWhiteSpace(address))
                                {
                                    string addr = await _cache.RetrieveAsync(address.ToLower());
                                    if (addr != null)
                                    {
                                        _logger.Debug($"Included Transaction: {JsonConvert.SerializeObject(transaction)}");
                                        var (isChange, bookBalanceDifference) = await IsChangeTransaction(transaction.Vin, address.ToLower());
                                        if (isChange)
                                        {
                                            var index = transaction.Vout.IndexOf(outTx);
                                            PublishBitcoinTransactionReceivedIntegrationEvent(outTx, transaction.TxId, index, true, bookBalanceDifference);
                                        }
                                        else
                                        {
                                            var index = transaction.Vout.IndexOf(outTx);
                                            PublishBitcoinTransactionReceivedIntegrationEvent(outTx, transaction.TxId, index);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Debug(e.ToString());
                    await _blockHasTransferChannel.AddBlockHashAsync(blockHash);
                    _logger.Debug("readded to queue: " + blockHash);
                }
            }
        }

        public async Task<(bool, decimal)> IsChangeTransaction(List<Vin> txIns, string changeAddress)
        {
            foreach (var txIn in txIns)
            {
                var cacheValue = await _cache.RetrieveAsync(txIn.TxId);
                if (!string.IsNullOrWhiteSpace(cacheValue)
                    && cacheValue.Contains("CHANGE_TRANSACTION"))
                {
                    var arr = cacheValue.Split(':');
                    if (arr[2].ToLower() != changeAddress.ToLower()) return (false, 0.00m);
                    var bookBalanceDifferenceString = arr[1];
                    var bookBalanceDifference = decimal.Parse(bookBalanceDifferenceString);
                    return (true, bookBalanceDifference);
                }
            }
            return (false, 0.00m);
        }

        private void PublishBitcoinTransactionReceivedIntegrationEvent(Vout vo, string txHash, int index, bool isChange = false, decimal bookBalanceDifference = 0.0m)
        {
            var @event = new BitcoinTransactionReceivedIntegrationEvent(vo, txHash, index, isChange, bookBalanceDifference);
            _eventBus.Publish(@event);
        }
    }
}
