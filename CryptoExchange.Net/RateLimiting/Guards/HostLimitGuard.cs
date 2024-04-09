using CryptoExchange.Net.RateLimiting.Trackers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Host limit guard, limit the amount of call to a certain host
    /// </summary>
    public class HostLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "HostLimitGuard";

        private readonly string _host;
        private WindowTracker _tracker;
        private RateLimitItemType _types;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="host"></param>
        /// <param name="limit"></param>
        /// <param name="timespan"></param>
        /// <param name="types"></param>
        public HostLimitGuard(string host, int limit, TimeSpan timespan, RateLimitItemType types = RateLimitItemType.Request) : base(limit, timespan)
        {
            _host = host;
            _types = types;
        }

        /// <inheritdoc />
        public LimitCheck Check(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!_types.HasFlag(type))
                return LimitCheck.NotApplicable;

            if (!string.Equals(url.Host, _host, StringComparison.OrdinalIgnoreCase))
                return LimitCheck.NotApplicable;

            if (_tracker == null)
                _tracker = CreateTracker();

            var delay = _tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, _tracker.Limit, _tracker.Timeperiod, _tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!_types.HasFlag(type))
                return RateLimitState.NotApplied;

            if (!string.Equals(url.Host, _host, StringComparison.OrdinalIgnoreCase))
                return RateLimitState.NotApplied;

            _tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(_tracker.Limit, _tracker.Timeperiod, _tracker.Current);
        }
    }
}
