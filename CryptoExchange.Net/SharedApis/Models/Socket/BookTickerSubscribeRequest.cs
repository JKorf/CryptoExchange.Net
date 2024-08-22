using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record BookTickerSubscribeRequest
    {
        public ApiType? ApiType { get; set; }
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }

        public BookTickerSubscribeRequest(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}
