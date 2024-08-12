using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
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
