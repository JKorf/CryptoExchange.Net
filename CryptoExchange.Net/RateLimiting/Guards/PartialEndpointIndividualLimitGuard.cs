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

        private readonly string _endpoint;
        private readonly Dictionary<string, WindowTracker> _trackers;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="limit"></param>
        /// <param name="timespan"></param>
        public PartialEndpointIndividualLimitGuard(string endpoint, int limit, TimeSpan timespan) : base(limit, timespan)
        {
            _endpoint = endpoint;
            _trackers = new Dictionary<string, WindowTracker>();
        }

        /// <inheritdoc />
        public LimitCheck Check(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return LimitCheck.NotApplicable;

            if (!_trackers.TryGetValue(url.AbsolutePath, out var tracker))
            {
                tracker = CreateTracker();
                _trackers[url.AbsolutePath] = tracker;
            }

            var delay = tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, tracker.Limit, tracker.Timeperiod, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!url.AbsolutePath.StartsWith(_endpoint))
                return RateLimitState.NotApplied;

            var tracker = _trackers[url.AbsolutePath];
            tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(tracker.Limit, tracker.Timeperiod, tracker.Current);
        }        
    }
}
