using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Socket
{
    public record SubscribeKlineRequest : SharedSymbolRequest
    {
        public SharedKlineInterval Interval { get; set; }

        public SubscribeKlineRequest(SharedSymbol symbol, SharedKlineInterval interval) : base(symbol)
        {
            Interval = interval;
        }
    }
}
