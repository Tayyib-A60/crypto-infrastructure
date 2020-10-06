using System;
using System.Collections.Generic;
using System.Text;

namespace WalletsCrypto.Bitcoin.Watcher.Models
{
    public class ExchangeRateResponse
    {
        public string asset_id_base { get; set; }
        public string asset_id_quote { get; set; }
        public decimal rate { get; set; }
    }
}
