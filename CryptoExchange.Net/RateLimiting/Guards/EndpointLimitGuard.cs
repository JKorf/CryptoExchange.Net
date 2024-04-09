using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Trackers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting
{
    /// <summary>
    /// Endpoint limit guard, limit the amount of calls to a specific endpoint
    /// </summary>
    public class EndpointLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "EndpointLimitGuard";

        private readonly string _endpoint;
        private WindowTracker? _tracker;
        private HttpMethod? _method;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="limit"></param>
        /// <param name="timespan"></param>
        /// <param name="method"></param>
        public EndpointLimitGuard(string endpoint, int limit, TimeSpan timespan, HttpMethod? method): base(limit, timespan)
        {
            _endpoint = endpoint;
            _method = method;
        }

        /// <inheritdoc />
        public LimitCheck Check(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!string.Equals(url.AbsolutePath, _endpoint, StringComparison.OrdinalIgnoreCase))
                return LimitCheck.NotApplicable;

            if (_method != null && _method != method)
                return LimitCheck.NotApplicable;

            if (_tracker == null)
                _tracker = CreateTracker();

            var delay = _tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, _tracker.Limit, _tracker.Timeperiod, _tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!string.Equals(url.AbsolutePath, _endpoint, StringComparison.OrdinalIgnoreCase))
                return RateLimitState.NotApplied;

            if (_method != null && _method != method)
                return RateLimitState.NotApplied;

            _tracker!.ApplyWeight(requestWeight);
            return RateLimitState.Applied(_tracker.Limit, _tracker.Timeperiod, _tracker.Current);
        }
    }
}
