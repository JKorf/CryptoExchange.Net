using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
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
        public IEnumerable<SharedKlineInterval> SupportIntervals { get; }
        /// <summary>
        /// Max number of data points which can be requested
        /// </summary>
        public int? MaxTotalDataPoints { get; set; }
        /// <summary>
        /// Max number of data points which can be requested in a single request
        /// </summary>
        public int? MaxRequestDataPoints { get; set; }
        /// <summary>
        /// The max age of the data that can be requested
        /// </summary>
        public TimeSpan? MaxAge { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetKlinesOptions(SharedPaginationSupport paginationType, bool needsAuthentication) : base(paginationType, needsAuthentication)
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

        /// <summary>
        /// ctor
        /// </summary>
        public GetKlinesOptions(SharedPaginationSupport paginationType, bool needsAuthentication, params SharedKlineInterval[] intervals) : base(paginationType, needsAuthentication)
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
                    if (((request.EndTime ?? DateTime.UtcNow) - request.StartTime.Value).TotalSeconds / (int)request.Interval > MaxTotalDataPoints.Value)
                        return new ArgumentError($"Only the most recent {MaxTotalDataPoints} klines are available, time filter failed");
                }
            }

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Supported SharedKlineInterval values: {string.Join(", ", SupportIntervals)}");
            if (MaxAge != null)
                sb.AppendLine($"Max age of data: {MaxAge}");
            if (MaxTotalDataPoints != null)
                sb.AppendLine($"Max total data points available: {MaxTotalDataPoints}");
            if (MaxRequestDataPoints != null)
                sb.AppendLine($"Max data points per request: {MaxRequestDataPoints}");
            return sb.ToString();
        }
    }
}
