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
    /// Partial endpoint total limit guard, limit the amount of requests to endpoints starting with a certain string, adding all requests matching to the same limit
    /// </summary>
    public class PartialEndpointTotalLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "PartialEndpointLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit of {Limit}[Requests] per {TimeSpan} for all endpoints starting with {_endpoint}";

        private readonly string _endpoint;
        private IWindowTracker? _tracker;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="limit"></param>
        /// <param name="timespan"></param>
        public PartialEndpointTotalLimitGuard(string endpoint, int limit, TimeSpan timespan): base(limit, timespan)
        {
            _endpoint = endpoint;
        }


        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!path.StartsWith(_endpoint))
                return LimitCheck.NotApplicable;

            if (_tracker == null)
                _tracker = CreateTracker();

            var delay = _tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, _tracker.Limit, _tracker.TimePeriod, _tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!path.StartsWith(_endpoint))
                return RateLimitState.NotApplied;

            _tracker!.ApplyWeight(requestWeight);
            return RateLimitState.Applied(_tracker.Limit, _tracker.TimePeriod, _tracker.Current);
        }
    }
}
