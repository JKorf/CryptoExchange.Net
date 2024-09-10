using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.SubscribeModels
{
    public record SubscribeTradeRequest : SharedSymbolRequest
    {
        public SubscribeTradeRequest(SharedSymbol symbol) : base(symbol)
        {
        }
    }
}
