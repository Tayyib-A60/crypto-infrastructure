using System;

namespace WalletsCrypto.Common.Configuration
{
    public class EthereumNodeConfiguration
    {
        public EthereumNodeConfiguration(string httpsUrl, string wssUrl)
        {
            if (string.IsNullOrWhiteSpace(httpsUrl))
                throw new ArgumentNullException(nameof(httpsUrl));

            if (string.IsNullOrWhiteSpace(wssUrl))
                throw new ArgumentNullException(nameof(wssUrl));

            HttpsUrl = httpsUrl;
            WssUrl = wssUrl;
        }
        public string HttpsUrl { get; }
        public string WssUrl { get; }

    }

}
