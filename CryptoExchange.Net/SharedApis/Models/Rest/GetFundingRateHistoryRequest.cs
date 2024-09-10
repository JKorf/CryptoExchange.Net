using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetFundingRateHistoryRequest : SharedSymbolRequest
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Limit { get; set; }

        public GetFundingRateHistoryRequest(SharedSymbol symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null) : base(symbol)
        {
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}
