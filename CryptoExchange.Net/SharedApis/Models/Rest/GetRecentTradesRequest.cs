using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net.SharedApis.Interfaces;
using CryptoExchange.Net.SharedApis.RequestModels;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetRecentTradesRequest : SharedSymbolRequest
    {
        public int? Limit { get; set; }

        public GetRecentTradesRequest(string baseAsset, string quoteAsset) : base(baseAsset, quoteAsset)
        {
        }

        public GetRecentTradesRequest(string symbol) : base(symbol)
        {
        }
    }
}
