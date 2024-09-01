using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetAssetRequest
    {
        public string Asset { get; set; }

        public GetAssetRequest(string asset)
        {
            Asset = asset;
        }
    }
}
