using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetDepositAddressesRequest : SharedRequest
    {
        public string Asset { get; set; }
        public string? Network { get; set; }

        public GetDepositAddressesRequest(string asset, string? network = null, ExchangeParameters? exchangeParameters = null): base(exchangeParameters)
        {
            Asset = asset;
            Network = network;
        }
    }
}
