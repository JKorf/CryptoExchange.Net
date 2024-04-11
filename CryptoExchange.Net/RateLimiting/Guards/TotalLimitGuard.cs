using CryptoExchange.Net.RateLimiting.Trackers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Total limit guard, limit the amount of total calls
    /// </summary>
    public class TotalLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "TotalLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit of {Limit}[Requests] per {TimeSpan}";

        private IWindowTracker? _tracker;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="timespan"></param>
        public TotalLimitGuard(int limit, TimeSpan timespan) : base(limit, timespan)
        {
        }


        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            _tracker ??= CreateTracker();
            var delay = _tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, _tracker.Limit, _tracker.TimePeriod, _tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            _tracker!.ApplyWeight(requestWeight);
            return RateLimitState.Applied(_tracker.Limit, _tracker.TimePeriod, _tracker.Current);
        }
    }
}
