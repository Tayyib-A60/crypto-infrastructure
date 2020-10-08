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
        private readonly BlockHashTransferChannel _blockHashTransferChannel;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public ProcessTransactionsService(
            ICacheStorage cache,
            IEventBus eventBus,
            BlockHashTransferChannel blockHashTransferChannel
            )
        {
            _cache = cache;
            _eventBus = _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _blockHashTransferChannel = blockHashTransferChannel;
            _coinService = new BitcoinService(ApplicationConfiguration.BitcoinNodeConfiguration.RPCUrl, 
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCUsername, 
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCPassword, 
                ApplicationConfiguration.BitcoinNodeConfiguration.WalletPassword, 
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCRequestTimeout); 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var blockHash in _blockHashTransferChannel.ReadAllAsync())
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
                                        var sender = transaction.Vout.Count > 1 ? transaction.Vout[1] : null;
                                        var (isChange, bookBalanceDifference, unTxHash) = await IsChangeTransaction(transaction.Vin, address.ToLower());
                                        if (isChange)
                                        {
                                            var index = transaction.Vout.IndexOf(outTx);
                                            PublishBitcoinTransactionReceivedIntegrationEvent(unTxHash, outTx, sender, transaction.TxId, index, true, bookBalanceDifference);
                                        }
                                        else
                                        {
                                            var index = transaction.Vout.IndexOf(outTx);
                                            PublishBitcoinTransactionReceivedIntegrationEvent(unTxHash, outTx, sender, transaction.TxId, index);
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
                    await _blockHashTransferChannel.AddBlockHashAsync(blockHash);
                    _logger.Debug("readded to queue: " + blockHash);
                }
            }
        }

        public async Task<(bool, decimal, string)> IsChangeTransaction(List<Vin> txIns, string changeAddress)
        {
            var unTxHash = String.Empty;
            foreach (var txIn in txIns)
            {
                var cacheValue = await _cache.RetrieveAsync(txIn.TxId);
                if (!string.IsNullOrWhiteSpace(cacheValue)
                    && cacheValue.Contains("CHANGE_TRANSACTION"))
                {
                    unTxHash = txIn.TxId;
                    var arr = cacheValue.Split(':');
                    if (arr[2].ToLower() != changeAddress.ToLower()) return (false, 0.00m, unTxHash);
                    var bookBalanceDifferenceString = arr[1];
                    var bookBalanceDifference = decimal.Parse(bookBalanceDifferenceString);
                    return (true, bookBalanceDifference, unTxHash);
                }
            }
            return (false, 0.00m, unTxHash);
        }

        private void PublishBitcoinTransactionReceivedIntegrationEvent(string unTxHash, Vout vo, Vout sender, string txHash, int index, bool isChange = false, decimal bookBalanceDifference = 0.0m)
        {
            var @event = new BitcoinTransactionReceivedIntegrationEvent(unTxHash, vo, sender, txHash, index, isChange, bookBalanceDifference);
            _eventBus.Publish(@event);
        }
    }
}
