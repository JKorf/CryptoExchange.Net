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
    /// API key limit guard, limit the amount of calls per API key
    /// </summary>
    public class ApiKeyLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "ApiKeyLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit of {Limit}[Requests] per {TimeSpan} per API key";

        private readonly Dictionary<string, IWindowTracker> _trackers;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="limit">The max count</param>
        /// <param name="timespan">The time period of the limit</param>
        public ApiKeyLimitGuard(int limit, TimeSpan timespan) : base(limit, timespan)
        {
            _trackers = new Dictionary<string, IWindowTracker>();
        }

        /// <inheritdoc />
        public LimitCheck Check(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (type != RateLimitItemType.Request || apiKey == null)
                return LimitCheck.NotApplicable;

            var ky = apiKey.GetString();
            if (!_trackers.TryGetValue(ky, out var tracker))
            {
                tracker = CreateTracker();
                _trackers[ky] = tracker;
            }

            var delay = tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, tracker.Limit, tracker.TimePeriod, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (apiKey == null)
                return RateLimitState.NotApplied;

            var ky = apiKey.GetString();
            var tracker = _trackers[ky];
            tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(tracker.Limit, tracker.TimePeriod, tracker.Current);
        }
    }
}
