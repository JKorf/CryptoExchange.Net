using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetPositionHistoryRequest
    {
        public ApiType? ApiType { get; set; }
        public SharedSymbol? Symbol { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Limit { get; set; }

        public GetPositionHistoryRequest(SharedSymbol? symbol)
        {
            Symbol = symbol;
        }

        public GetPositionHistoryRequest(ApiType apiType)
        {
            ApiType = apiType;
        }
    }
}
