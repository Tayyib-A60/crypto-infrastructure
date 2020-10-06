using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WalletsCrypto.Ethereum.Watcher.Channels
{
    public class BlockNumberTransferChannel
    {
        private readonly Channel<HexBigInteger> _channel;
        private readonly ILogger<BlockNumberTransferChannel> _logger;

        public BlockNumberTransferChannel(ILogger<BlockNumberTransferChannel> logger)
        {
            var options = new BoundedChannelOptions(int.MaxValue)
            {
                SingleWriter = false,
                SingleReader = true
            };

            _channel = Channel.CreateBounded<HexBigInteger>(options);
            _logger = logger;
        }

        public async Task AddBlockNumberAsync(HexBigInteger blockNumber, CancellationToken ct = default)
        {
            await _channel.Writer.WriteAsync(blockNumber);
        }

        public IAsyncEnumerable<HexBigInteger> ReadAllAsync(CancellationToken ct = default) =>
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
