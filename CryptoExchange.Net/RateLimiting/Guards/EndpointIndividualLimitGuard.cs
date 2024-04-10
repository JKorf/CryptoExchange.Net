using CryptoExchange.Net.Objects;
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
    public class EndpointIndividualLimitGuard
    {
        private readonly Dictionary<string, IWindowTracker> _trackers;
        private readonly RateLimitWindowType _type;

        /// <summary>
        /// ctor
        /// </summary>
        public EndpointIndividualLimitGuard(RateLimitWindowType type)
        {
            _trackers = new Dictionary<string, IWindowTracker>();
            _type = type;
        }

        /// <inheritdoc />
        public LimitCheck Check(string key, int limit, TimeSpan period, int requestWeight)
        {
            if (!_trackers.TryGetValue(key, out var tracker))
            {
                tracker = _type == RateLimitWindowType.Sliding? new SlidingWindowTracker(limit, period) : new FixedWindowTracker(limit, period);
                _trackers[key] = tracker;
            }

            var delay = tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, tracker.Limit, tracker.TimePeriod, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(string key, int weight)
        {
            var tracker = _trackers[key];
            tracker.ApplyWeight(weight);
            return RateLimitState.Applied(tracker.Limit, tracker.TimePeriod, tracker.Current);
        }        
    }
}
