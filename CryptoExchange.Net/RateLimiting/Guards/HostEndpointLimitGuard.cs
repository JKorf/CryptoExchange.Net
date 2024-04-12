using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Host limit guard, limit the amount of call to a certain host
    /// </summary>
    public class HostEndpointLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "HostEndpointLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit of {Limit}[{string.Join(",", _types)}] per {TimeSpan} per endpoint to host {_host}";

        private readonly string _host;
        private readonly RateLimitItemType _types;
        private readonly Dictionary<string, IWindowTracker> _trackers;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="host"></param>
        /// <param name="limit"></param>
        /// <param name="timespan"></param>
        /// <param name="types"></param>
        public HostEndpointLimitGuard(string host, int limit, TimeSpan timespan, RateLimitItemType types = RateLimitItemType.Request) : base(limit, timespan)
        {
            _host = host;
            _types = types;
            _trackers = new Dictionary<string, IWindowTracker>();
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!_types.HasFlag(type))
                return LimitCheck.NotApplicable;

            if (!string.Equals(host, _host, StringComparison.OrdinalIgnoreCase))
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
            if (!_types.HasFlag(type))
                return RateLimitState.NotApplied;

            if (!string.Equals(host, _host, StringComparison.OrdinalIgnoreCase))
                return RateLimitState.NotApplied;

            var tracker = _trackers[path];
            tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(tracker.Limit, tracker.TimePeriod, tracker.Current);
        }
    }
}
