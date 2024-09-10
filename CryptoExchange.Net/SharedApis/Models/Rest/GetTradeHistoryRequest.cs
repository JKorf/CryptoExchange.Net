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
        public int? Limit { get; }

        public GetTradeHistoryRequest(SharedSymbol symbol, DateTime startTime, DateTime endTime, int? limit = null) : base(symbol)
        {
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}
