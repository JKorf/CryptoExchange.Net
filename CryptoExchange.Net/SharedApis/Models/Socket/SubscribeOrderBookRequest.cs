using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeOrderBookRequest : SharedSymbolRequest
    {
        public int? Limit { get; set; }

        public SubscribeOrderBookRequest(SharedSymbol symbol, int? limit = null) : base(symbol)
        {
            Limit = limit;
        }
    }
}
