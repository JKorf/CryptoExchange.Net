using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetAssetRequest : SharedRequest
    {
        public string Asset { get; set; }

        public GetAssetRequest(string asset, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Asset = asset;
        }
    }
}
