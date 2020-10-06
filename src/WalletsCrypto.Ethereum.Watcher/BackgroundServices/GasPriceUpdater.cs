using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using System;
using System.Threading;
using System.Threading.Tasks;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Infrastructure.Cache;

namespace WalletsCrypto.Ethereum.Watcher.BackgroundServices
{
    public class GasPriceUpdater : BackgroundService
    {
        private readonly Web3 _web3;
        private readonly ICacheStorage _cache;
        private readonly ILogger<GasPriceUpdater> _logger;
        public GasPriceUpdater(
            ICacheStorage cache,
            ILogger<GasPriceUpdater> logger)
        {
            _cache = cache;
            _logger = logger;
            _web3 = new Web3(ApplicationConfiguration.EthereumNodeConfiguration.HttpsUrl);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var gasPriceWei = await _web3.Eth.GasPrice.SendRequestAsync();
                    var gasPrice = Web3.Convert.FromWei(gasPriceWei, Nethereum.Util.UnitConversion.EthUnit.Ether);
                    await _cache.StoreAsync("CURRENT_ETHEREUM_GAS_PRICE", gasPrice.ToString());
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
                catch (Exception)
                {

                }
               
            }
        }
    }
}
