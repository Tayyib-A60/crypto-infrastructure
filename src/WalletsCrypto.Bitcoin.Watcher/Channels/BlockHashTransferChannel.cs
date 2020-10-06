using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WalletsCrypto.Bitcoin.Watcher.Channels
{
    public class BlockHashTransferChannel
    {
        private readonly Channel<string> _channel;
        private readonly ILogger<BlockHashTransferChannel> _logger;

        public BlockHashTransferChannel(
            ILogger<BlockHashTransferChannel> logger)
        {
            _logger = logger;
            var options = new BoundedChannelOptions(int.MaxValue)
            {
                SingleWriter = false,
                SingleReader = true
            };
            _channel = Channel.CreateBounded<string>(options);
        }

        public async Task AddBlockHashAsync(string blockHash, CancellationToken ct = default)
        {
            await _channel.Writer.WriteAsync(blockHash);
        }

        public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct = default) =>
            _channel.Reader.ReadAllAsync(ct);

        public bool TryCompleteWriter(Exception ex = null) => _channel.Writer.TryComplete(ex);

        internal static class EventIds
        {
            public static readonly EventId ChannelMessageWritten = new EventId(100, "ChannelMessageWritten");
        }

        private static class Log
        {
            private static readonly Action<ILogger, string, Exception> _channelMessageWritten = LoggerMessage.Define<string>(
                LogLevel.Information,
                EventIds.ChannelMessageWritten,
                "Filename {FileName} was written to the channel.");

            public static void ChannelMessageWritten(ILogger logger, string fileName)
            {
                _channelMessageWritten(logger, fileName, null);
            }
        }
    }
}
