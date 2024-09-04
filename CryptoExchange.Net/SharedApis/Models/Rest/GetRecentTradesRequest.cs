using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Interfaces;
using CryptoExchange.Net.SharedApis.RequestModels;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetRecentTradesRequest : SharedSymbolRequest
    {
        public int? Limit { get; set; }

        public GetRecentTradesRequest(ApiType apiType, SharedSymbol symbol) : base(symbol, apiType)
        {
        }
    }
}
