using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record GetDepositAddressesRequest
    {
        public string Asset { get; set; }

        public string? Network { get; set; }

        public GetDepositAddressesRequest(string asset)
        {
            Asset = asset;
        }
    }
}
