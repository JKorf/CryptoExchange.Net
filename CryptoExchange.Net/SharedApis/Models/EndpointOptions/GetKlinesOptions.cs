using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetKlinesOptions : PaginatedEndpointOptions<GetKlinesRequest>
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

        public override Error? ValidateRequest(string exchange, GetKlinesRequest request, ExchangeParameters? exchangeParameters, ApiType apiType, ApiType[] supportedApiTypes)
        {
            if (!IsSupported(request.Interval))
                return new ArgumentError("Interval not supported");

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return new ArgumentError($"Only the most recent {MaxAge} klines are available");

             if (MaxRequestDataPoints.HasValue && request.Limit > MaxRequestDataPoints.Value)
                    return new ArgumentError($"Only {MaxRequestDataPoints} klines can be retrieved per request");
            
            if (MaxTotalDataPoints.HasValue)
            {
                if (request.Limit > MaxTotalDataPoints.Value)
                    return new ArgumentError($"Only the most recent {MaxTotalDataPoints} klines are available");

                if (request.StartTime.HasValue == true)
                {
                    if ((request.EndTime!.Value - request.StartTime.Value).TotalSeconds / (int)request.Interval > MaxTotalDataPoints.Value)
                        return new ArgumentError($"Only the most recent {MaxTotalDataPoints} klines are available, time filter failed");
                }
            }

            return base.ValidateRequest(exchange, request, exchangeParameters, apiType, supportedApiTypes);
        }
    }
}
