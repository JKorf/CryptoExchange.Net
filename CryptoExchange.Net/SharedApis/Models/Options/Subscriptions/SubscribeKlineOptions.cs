using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to kline/candlestick updates
    /// </summary>
    public class SubscribeKlineOptions : EndpointOptions<SubscribeKlineRequest, IKlineSocketClient>
    {
        /// <summary>
        /// Kline intervals supported for updates
        /// </summary>
        public SharedKlineInterval[] SupportIntervals { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeKlineOptions(string exchange, bool needsAuthentication) : base(exchange, needsAuthentication, nameof(IKlineSocketClient.SubscribeToKlineUpdatesAsync))
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
        public SubscribeKlineOptions(string exchange, bool needsAuthentication, params SharedKlineInterval[] intervals) 
            : base(exchange, needsAuthentication, nameof(IKlineSocketClient.SubscribeToKlineUpdatesAsync))
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
        public override Error? ValidateRequest(SubscribeKlineRequest request, IKlineSocketClient client)
        {
            if (!IsSupported(request.Interval))
                return ArgumentError.Invalid(nameof(SubscribeKlineRequest.Interval), "Interval not supported");

            return base.ValidateRequest(request, client);
        }
    }
}
