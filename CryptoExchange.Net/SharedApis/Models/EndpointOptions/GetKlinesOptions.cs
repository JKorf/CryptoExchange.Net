using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetKlinesOptions : PaginatedEndpointOptions
    {
        public IEnumerable<SharedKlineInterval> SupportIntervals { get; }
        public int? MaxTotalDataPoints { get; set; }
        public int? MaxRequestDataPoints { get; set; }
        public TimeSpan? MaxAge { get; set; }

        public GetKlinesOptions(bool paginationSupport, bool needsAuthentication) : base(paginationSupport, needsAuthentication)
        {
            SupportIntervals = new[]
            {
                SharedKlineInterval.FiveMinutes,
                SharedKlineInterval.FifteenMinutes,
                SharedKlineInterval.OneHour,
                SharedKlineInterval.FifteenMinutes,
                SharedKlineInterval.OneDay,
                SharedKlineInterval.OneWeek,
                SharedKlineInterval.OneMonth
            };
        }

        public GetKlinesOptions(bool paginationSupport, bool needsAuthentication, params SharedKlineInterval[] intervals) : base(paginationSupport, needsAuthentication)
        {
            SupportIntervals = intervals;
        }

        public bool IsSupported(SharedKlineInterval interval) => SupportIntervals.Contains(interval);

        public Error? Validate(GetKlinesRequest request)
        {
            if (!IsSupported(request.Interval))
                return new ArgumentError("Interval not supported");

            if (MaxAge.HasValue && request.Filter?.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return new ArgumentError($"Only the most recent {MaxAge} klines are available");

             if (MaxRequestDataPoints.HasValue && request.Filter?.Limit > MaxRequestDataPoints.Value)
                    return new ArgumentError($"Only {MaxRequestDataPoints} klines can be retrieved per request");
            
            if (MaxTotalDataPoints.HasValue)
            {
                if (request.Filter?.Limit > MaxTotalDataPoints.Value)
                    return new ArgumentError($"Only the most recent {MaxTotalDataPoints} klines are available");

                if (request.Filter?.StartTime.HasValue == true)
                {
                    if ((request.Filter.EndTime!.Value - request.Filter.StartTime.Value).TotalSeconds / (int)request.Interval > MaxTotalDataPoints.Value)
                        return new ArgumentError($"Only the most recent {MaxTotalDataPoints} klines are available, time filter failed");
                }
            }

            return null;
        }
    }
}
