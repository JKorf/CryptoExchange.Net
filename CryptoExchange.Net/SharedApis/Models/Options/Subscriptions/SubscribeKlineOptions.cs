using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to kline/candlestick updates
    /// </summary>
    public class SubscribeKlineOptions : EndpointOptions<SubscribeKlineRequest>
    {
        /// <summary>
        /// Kline intervals supported for updates
        /// </summary>
        public IEnumerable<SharedKlineInterval> SupportIntervals { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeKlineOptions(bool needsAuthentication) : base(needsAuthentication)
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
        public SubscribeKlineOptions(bool needsAuthentication, params SharedKlineInterval[] intervals) : base(needsAuthentication)
        {
            SupportIntervals = intervals;
        }

        /// <summary>
        /// Check whether a specific interval is supported
        /// </summary>
        /// <param name="interval">Interval</param>
        /// <returns></returns>
        public bool IsSupported(SharedKlineInterval interval) => SupportIntervals.Contains(interval);

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(string exchange, SubscribeKlineRequest request, TradingMode? tradingMode, TradingMode[] supportedApiTypes)
        {
            if (!IsSupported(request.Interval))
                return new ArgumentError("Interval not supported");

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }
    }
}
