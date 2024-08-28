using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetSpotOpenOrdersRequest
    {
        public string? Symbol { get; }
        public string? BaseAsset { get; }
        public string? QuoteAsset { get; }


        public GetSpotOpenOrdersRequest()
        {
        }

        public GetSpotOpenOrdersRequest(string symbol)
        {
            Symbol = symbol;
        }

        public GetSpotOpenOrdersRequest(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }

        public string? GetSymbol(Func<string, string, ApiType?, string> format)
        {
            if (!string.IsNullOrEmpty(Symbol))
                return Symbol;

            if (string.IsNullOrEmpty(BaseAsset))
                return null;

            return format(BaseAsset, QuoteAsset, ApiType.Spot);
        }
    }
}
