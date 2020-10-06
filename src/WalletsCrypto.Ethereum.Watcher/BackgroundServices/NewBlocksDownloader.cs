using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.WebSockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Ethereum.Watcher.Channels;
using Websocket.Client;

namespace WalletsCrypto.Ethereum.Watcher.BackgroundServices
{
    public class NewBlocksDownloader : BackgroundService
    {
        private readonly ILogger<NewBlocksDownloader> _logger;
        private readonly BlockNumberTransferChannel _transactionsTransferChannel;
        private readonly Func<ClientWebSocket> _webSocketFactory;
        private HexBigInteger _lastBlockNumber = new HexBigInteger(new BigInteger(0));

        public NewBlocksDownloader(
            ILogger<NewBlocksDownloader> logger,
            BlockNumberTransferChannel transactionsTransferChannel,
            Func<ClientWebSocket> webSocketFactory)
        {
            _logger = logger;
            _webSocketFactory = webSocketFactory;
            _transactionsTransferChannel = transactionsTransferChannel;
        }

     
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var request = new { jsonrpc = "2.0", Id = Guid.NewGuid().ToString(), method = "eth_subscribe", @params = new object[] { "newHeads" } };
            var requestString = JsonConvert.SerializeObject(request);

            var client = new WebsocketClient(new Uri(ApplicationConfiguration.EthereumNodeConfiguration.WssUrl), _webSocketFactory);
            client.ReconnectTimeout = TimeSpan.FromMinutes(5);
            client.ReconnectionHappened.Subscribe((msg) =>
            {
                client.Send(requestString);
            });


            client.MessageReceived.Subscribe(async msg =>
            {
                var res = JObject.Parse(msg.Text);
                if(res["id"] == null)
                {
                    var number = res["params"]["result"]["number"];
                    var blockNumber = new HexBigInteger(number.ToObject<string>());
                    if(blockNumber.Value > _lastBlockNumber.Value)
                    {
                        _lastBlockNumber = blockNumber;
                        await _transactionsTransferChannel.AddBlockNumberAsync(blockNumber);

                    }
                }
                else
                    _logger.LogInformation($"Subscribed to websocket with Id: {msg.Text}");
            });



            await client.Start();

            while(!stoppingToken.IsCancellationRequested)
            {
                if(!client.IsRunning)
                {
                    _logger.LogInformation($"{DateTime.Now} - client disconected");
                    await  client.Reconnect();
                }
                await Task.Delay(TimeSpan.FromMinutes(50));
                client.Send("ping");
                _logger.LogInformation($"{DateTime.Now} - pinged websocket - client running: {client.IsRunning}");
            }
            
            
        }
    }

}
