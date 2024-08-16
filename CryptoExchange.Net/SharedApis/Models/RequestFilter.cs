using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models
{
    public class RequestFilter
    {
        public DateTime? StartTime { get; }
        public DateTime? EndTime { get; }
        public int? Limit { get; }

        public RequestFilter(DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public RequestFilter(int limit)
        {
            Limit = limit;
        }
    }
}
