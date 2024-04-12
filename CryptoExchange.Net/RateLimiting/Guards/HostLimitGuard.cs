using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Net.Http;
using System.Security;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Host limit guard, limit the amount of call to a certain host
    /// </summary>
    public class HostLimitGuard : LimitGuard, IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "HostLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit of {Limit}[{string.Join(",", _types)}] per {TimeSpan} to host {_host}";

        private readonly string _host;
        private IWindowTracker? _tracker;
        private readonly RateLimitItemType _types;

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
        public LimitCheck Check(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!_types.HasFlag(type))
                return LimitCheck.NotApplicable;

            if (!string.Equals(host, _host, StringComparison.OrdinalIgnoreCase))
                return LimitCheck.NotApplicable;

            if (_tracker == null)
                _tracker = CreateTracker();

            var delay = _tracker.GetWaitTime(requestWeight);
            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, _tracker.Limit, _tracker.TimePeriod, _tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            if (!_types.HasFlag(type))
                return RateLimitState.NotApplied;

            if (!string.Equals(host, _host, StringComparison.OrdinalIgnoreCase))
                return RateLimitState.NotApplied;

            _tracker!.ApplyWeight(requestWeight);
            return RateLimitState.Applied(_tracker.Limit, _tracker.TimePeriod, _tracker.Current);
        }
    }
}
