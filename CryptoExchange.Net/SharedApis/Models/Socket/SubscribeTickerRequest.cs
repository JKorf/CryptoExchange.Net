using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeTickerRequest : SharedSymbolRequest
    {
        public SubscribeTickerRequest(SharedSymbol symbol) : base(symbol)
        {
        }
    }
}
