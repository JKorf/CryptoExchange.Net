using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetTradeHistoryRequest : SharedSymbolRequest
    {
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }

        public GetTradeHistoryRequest(ApiType apiType, SharedSymbol symbol, DateTime startTime, DateTime endTime) : base(symbol, apiType)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
