using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetClosedOrdersRequest : SharedSymbolRequest
    {
        public DateTime? StartTime { get; }
        public DateTime? EndTime { get; }
        public int? Limit { get; }

        public GetClosedOrdersRequest(ApiType apiType, SharedSymbol symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null) : base(symbol, apiType)
        {
        }
    }
}
