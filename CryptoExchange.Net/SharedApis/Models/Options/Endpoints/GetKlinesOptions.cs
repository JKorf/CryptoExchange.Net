using CryptoExchange.Net.Objects;
using System;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting kline/candlestick data
    /// </summary>
    public class GetKlinesOptions : PaginatedEndpointOptions<GetKlinesRequest>
    {
        /// <summary>
        /// The supported kline intervals
        /// </summary>
        public SharedKlineInterval[] SupportIntervals { get; }
        /// <summary>
        /// Max number of data points which can be requested
        /// </summary>
        public int? MaxTotalDataPoints { get; set; }
        /// <summary>
        /// The max age of the data that can be requested
        /// </summary>
        public TimeSpan? MaxAge { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetKlinesOptions(bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit, bool needsAuthentication)
            : base(supportsAscending, supportsDescending, timeFilterSupported, maxLimit, needsAuthentication)
        {
            SupportIntervals = new[]
            {
                SharedKlineInterval.OneMinute,
                SharedKlineInterval.ThreeMinutes,
                SharedKlineInterval.FiveMinutes,
                SharedKlineInterval.FifteenMinutes,
                SharedKlineInterval.ThirtyMinutes,
                SharedKlineInterval.OneHour,
                SharedKlineInterval.TwoHours,
                SharedKlineInterval.FourHours,
                SharedKlineInterval.SixHours,
                SharedKlineInterval.EightHours,
                SharedKlineInterval.TwelveHours,
                SharedKlineInterval.OneDay,
                SharedKlineInterval.OneWeek,
                SharedKlineInterval.OneMonth
            };
        }

        /// <summary>
        /// ctor
        /// </summary>
        public GetKlinesOptions(bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit, bool needsAuthentication, params SharedKlineInterval[] intervals) 
            : base(supportsAscending, supportsDescending, timeFilterSupported, maxLimit, needsAuthentication)
        {
            SupportIntervals = intervals;
        }

        /// <summary>
        /// Check whether a specific interval is supported
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool IsSupported(SharedKlineInterval interval) => SupportIntervals.Contains(interval);

        /// <inheritdoc />
        public override Error? ValidateRequest(string exchange, GetKlinesRequest request, TradingMode? tradingMode, TradingMode[] supportedApiTypes)
        {
            if (!IsSupported(request.Interval))
                return ArgumentError.Invalid(nameof(GetKlinesRequest.Interval), "Interval not supported");

            if (!SupportsAscending && request.Direction == DataDirection.Ascending)
                return ArgumentError.Invalid(nameof(GetWithdrawalsRequest.Direction), $"Ascending direction is not supported");

            if (!SupportsDescending && request.Direction == DataDirection.Descending)
                return ArgumentError.Invalid(nameof(GetWithdrawalsRequest.Direction), $"Descending direction is not supported");

            if (!TimePeriodFilterSupport)
            {
                // When going descending we can still allow startTime filter to limit the results
                var now = DateTime.UtcNow;
                if ((request.Direction != DataDirection.Descending && request.StartTime != null)
                    || (request.EndTime != null && now - request.EndTime > TimeSpan.FromSeconds(5)))
                {
                    return ArgumentError.Invalid(nameof(GetDepositsRequest.StartTime), $"Time filter is not supported");
                }
            }

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return ArgumentError.Invalid(nameof(GetKlinesRequest.StartTime), $"Only the most recent {MaxAge} klines are available");

            if (request.Limit > MaxLimit)
                return ArgumentError.Invalid(nameof(GetKlinesRequest.Limit), $"Only {MaxLimit} klines can be retrieved per request");

            if (MaxTotalDataPoints.HasValue)
            {
                if (request.Limit > MaxTotalDataPoints.Value)
                    return ArgumentError.Invalid(nameof(GetKlinesRequest.Limit), $"Only the most recent {MaxTotalDataPoints} klines are available");

                if (request.StartTime.HasValue == true)
                {
                    if (((request.EndTime ?? DateTime.UtcNow) - request.StartTime.Value).TotalSeconds / (int)request.Interval > MaxTotalDataPoints.Value)
                        return ArgumentError.Invalid(nameof(GetKlinesRequest.StartTime), $"Only the most recent {MaxTotalDataPoints} klines are available, time filter failed");
                }
            }

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Time filter supported: {TimePeriodFilterSupport}");
            sb.AppendLine($"Supported SharedKlineInterval values: {string.Join(", ", SupportIntervals)}");
            if (MaxAge != null)
                sb.AppendLine($"Max age of data: {MaxAge}");
            if (MaxTotalDataPoints != null)
                sb.AppendLine($"Max total data points available: {MaxTotalDataPoints}");
            return sb.ToString();
        }
    }
}
