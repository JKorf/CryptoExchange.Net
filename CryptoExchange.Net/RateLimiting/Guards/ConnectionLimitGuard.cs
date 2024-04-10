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
    /// Connection limit guard, limit the amount of connections to a certain host
    /// </summary>
    public class ConnectionLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "ConnectionLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit of {Limit}[Connections] per {TimeSpan} to host {_host}";

        private readonly string _host;
        private IWindowTracker? _tracker;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="host"></param>
        /// <param name="limit"></param>
        /// <param name="timespan"></param>
        public ConnectionLimitGuard(string host, int limit, TimeSpan timespan) : base(limit, timespan)
        {
            _host = host;
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (type != RateLimitItemType.Connection)
                return LimitCheck.NotApplicable;

            if (!string.Equals(url.Host, _host, StringComparison.OrdinalIgnoreCase))
                return LimitCheck.NotApplicable;

            if (_tracker == null)
                _tracker = CreateTracker();

            var delay = _tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, _tracker.Limit, _tracker.TimePeriod, _tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (type != RateLimitItemType.Connection)
                return RateLimitState.NotApplied;

            if (!string.Equals(url.Host, _host, StringComparison.OrdinalIgnoreCase))
                return RateLimitState.NotApplied;

            _tracker!.ApplyWeight(1);
            return RateLimitState.Applied(_tracker.Limit, _tracker.TimePeriod, _tracker.Current);
        }
    }
}
