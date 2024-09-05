using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetDepositsRequest
    {
        public string? Asset { get; set; }
        public DateTime? StartTime { get; }
        public DateTime? EndTime { get; }
        public int? Limit { get; }

        public GetDepositsRequest(string? asset = null, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            Asset = asset;
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}
