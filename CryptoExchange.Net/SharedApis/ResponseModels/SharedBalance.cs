using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedBalance
    {
        public string Asset { get; set; }
        public decimal Available { get; set; }
        public decimal Total { get; set; }
        public string? IsolatedMarginAsset { get; set; }

        public SharedBalance(string asset, decimal available, decimal total)
        {
            Asset = asset;
            Available = available;
            Total = total;
        }
    }
}
