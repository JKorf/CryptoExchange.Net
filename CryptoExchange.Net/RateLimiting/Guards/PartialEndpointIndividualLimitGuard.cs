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
    /// Partial endpoint individual limit guard, limit the amount of requests to endpoints starting with a certain string on an individual endpoint basis
    /// </summary>
    public class PartialEndpointIndividualLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "PartialEndpointIndividualLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit of {Limit}[Requests] per {TimeSpan} for each endpoint starting with {_endpoint}";

        private readonly string _endpoint;
        private readonly Dictionary<string, IWindowTracker> _trackers;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="limit"></param>
        /// <param name="timespan"></param>
        public PartialEndpointIndividualLimitGuard(string endpoint, int limit, TimeSpan timespan) : base(limit, timespan)
        {
            _endpoint = endpoint;
            _trackers = new Dictionary<string, IWindowTracker>();
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!path.StartsWith(_endpoint))
                return LimitCheck.NotApplicable;

            if (!_trackers.TryGetValue(path, out var tracker))
            {
                tracker = CreateTracker();
                _trackers[path] = tracker;
            }

            var delay = tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, tracker.Limit, tracker.TimePeriod, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!path.StartsWith(_endpoint))
                return RateLimitState.NotApplied;

            var tracker = _trackers[path];
            tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(tracker.Limit, tracker.TimePeriod, tracker.Current);
        }        
    }
}
