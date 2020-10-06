using BitcoinLib.Services.Coins.Base;
using BitcoinLib.Services.Coins.Bitcoin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WalletsCrypto.Bitcoin.Watcher.Models;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Infrastructure.Cache;
using System.Configuration;

namespace WalletsCrypto.Bitcoin.Watcher.BackgroundServices
{
    public class TransactionFeeUpdater : BackgroundService
    {
        private readonly ICacheStorage _cache;
        private readonly ICoinService _coinService;
        private readonly ILogger<TransactionFeeUpdater> _logger;
        private readonly string _coinApiUrl = ConfigurationManager.AppSettings["CoinApiUrl"];
        private readonly string _coinApiKey = ConfigurationManager.AppSettings["CoinApiKey"];
        public TransactionFeeUpdater(
            ICacheStorage cache,
            ILogger<TransactionFeeUpdater> logger)
        {
            _cache = cache;
            _logger = logger;
            _coinService = new BitcoinService(ApplicationConfiguration.BitcoinNodeConfiguration.RPCUrl,
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCUsername,
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCPassword,
                ApplicationConfiguration.BitcoinNodeConfiguration.WalletPassword,
                ApplicationConfiguration.BitcoinNodeConfiguration.RPCRequestTimeout);
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var fee = _coinService.EstimateSmartFee(1008);
                    await _cache.StoreAsync("CURRENT_BITCOIN_TRANSACTION_FEE", fee.FeeRate.ToString());

                    // Store Exchange Rate
                    var exchangeRate = await GetCurrentBTCExchangeRate();
                    await _cache.StoreAsync("BITCOIN_USD_RATE", exchangeRate.rate.ToString());

                    await Task.Delay(TimeSpan.FromMinutes(10));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
                
            }
        }

        private async Task<ExchangeRateResponse> GetCurrentBTCExchangeRate()
        {
            var httpClient = new HttpClient();

            HttpResponseMessage httpResponse = await httpClient.GetAsync($"{_coinApiUrl}BTC/USD?apikey={_coinApiKey}").ConfigureAwait(false);

            return await Response<ExchangeRateResponse>(httpResponse);
        }

        private async Task<T> Response<T>(HttpResponseMessage httpResponse)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var responseStr = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<T>(responseStr);
            }
            else if (httpResponse.StatusCode == HttpStatusCode.BadGateway)
            {
                throw new ServiceDowntimeException("Unable to reach service provider");
            }
            else if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                return default;

            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            throw new ServiceDowntimeException(content);
        }
    }
}
