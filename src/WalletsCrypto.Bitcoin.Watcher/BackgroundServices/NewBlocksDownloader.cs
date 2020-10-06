using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Common.Extensions;
using System.Text;
using WalletsCrypto.Bitcoin.Watcher.Channels;

namespace WalletsCrypto.Bitcoin.Watcher.BackgroundServices
{
    public class NewBlocksDownloader : BackgroundService
    {
        private readonly ILogger<NewBlocksDownloader> _logger;
        private readonly BlockHashTransferChannel _blockHashTransferChannel;

        public NewBlocksDownloader(
            ILogger<NewBlocksDownloader> logger,
            BlockHashTransferChannel blockHashTransferChannel)
        {
            _logger = logger;
            _blockHashTransferChannel = blockHashTransferChannel;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var subscriber = new SubscriberSocket())
            {
                subscriber.Connect(ApplicationConfiguration.BitcoinNodeConfiguration.ZMQUrl);
                subscriber.Subscribe("hashblock");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var byteArrayList = subscriber.ReceiveMultipartBytes(3);
                        var topic = Encoding.Default.GetString(byteArrayList[0]);
                        var hash = byteArrayList[1].GetString();
                        var length = BitConverter.ToInt32(byteArrayList[2]);
                        await _blockHashTransferChannel.AddBlockHashAsync(hash);

                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e.Message);
                    }
                    
                }
            }
        }
    }
}
