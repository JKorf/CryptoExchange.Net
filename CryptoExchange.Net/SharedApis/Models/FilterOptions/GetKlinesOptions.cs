using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetKlinesOptions : EndpointOptions
    {
        public IEnumerable<SharedKlineInterval> SupportIntervals { get; }

        public GetKlinesOptions(bool timeFilterSupport, bool paginationSupport, params SharedKlineInterval[] intervals) : base(timeFilterSupport, paginationSupport)
        {
            SupportIntervals = intervals;
        }

        public bool IsSupported(SharedKlineInterval interval) => SupportIntervals.Contains(interval);
    }
}
