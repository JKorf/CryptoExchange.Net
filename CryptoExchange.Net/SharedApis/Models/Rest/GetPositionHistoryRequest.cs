using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetPositionHistoryRequest : SharedRequest
    {
        public TradingMode? ApiType { get; set; }
        public SharedSymbol? Symbol { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Limit { get; set; }

        public GetPositionHistoryRequest(SharedSymbol symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Symbol = symbol;
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }

        public GetPositionHistoryRequest(TradingMode? apiType = null, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ApiType = apiType;
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}
