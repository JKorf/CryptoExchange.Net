using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeBookTickerRequest : SharedSymbolRequest
    {
        public SubscribeBookTickerRequest(ApiType apiType, SharedSymbol symbol) : base(symbol, apiType)
        {
        }
    }
}
