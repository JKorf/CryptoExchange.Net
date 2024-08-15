using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net.SharedApis.RequestModels;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetTickerRequest : SharedSymbolRequest
    {

        public GetTickerRequest(string baseAsset, string quoteAsset) : base(baseAsset, quoteAsset)
        {
        }

        public GetTickerRequest(string symbol) : base(symbol)
        {
        }
    }
}
