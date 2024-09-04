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

        public GetFundingRateHistoryRequest(ApiType apiType, SharedSymbol symbol) : base(symbol, apiType)
        {
        }
    }
}
