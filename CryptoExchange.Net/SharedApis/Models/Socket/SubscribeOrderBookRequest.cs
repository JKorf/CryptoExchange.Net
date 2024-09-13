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

        public SubscribeOrderBookRequest(SharedSymbol symbol, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            Limit = limit;
        }
    }
}
