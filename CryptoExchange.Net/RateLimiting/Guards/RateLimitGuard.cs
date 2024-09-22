using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <inheritdoc />
    public class RateLimitGuard : IRateLimitGuard
    {
        /// <summary>
        /// Apply guard per host
        /// </summary>
        public static Func<RequestDefinition, string, string?, string> PerHost { get; } = new Func<RequestDefinition, string, string?, string>((def, host, key) => host);
        /// <summary>
        /// Apply guard per endpoint
        /// </summary>
        public static Func<RequestDefinition, string, string?, string> PerEndpoint { get; } = new Func<RequestDefinition, string, string?, string>((def, host, key) => def.Path + def.Method);
        /// <summary>
        /// Apply guard per API key
        /// </summary>
        public static Func<RequestDefinition, string, string?, string> PerApiKey { get; } = new Func<RequestDefinition, string, string?, string>((def, host, key) => key!);
        /// <summary>
        /// Apply guard per API key per endpoint
        /// </summary>
        public static Func<RequestDefinition, string, string?, string> PerApiKeyPerEndpoint { get; } = new Func<RequestDefinition, string, string?, string>((def, host, key) => key! + def.Path + def.Method);

        private readonly IEnumerable<IGuardFilter> _filters;
        private readonly Dictionary<string, IWindowTracker> _trackers;
        private RateLimitWindowType _windowType;
        private double? _decayRate;
        private int? _connectionWeight;
        private readonly Func<RequestDefinition, string, string?, string> _keySelector;

        /// <inheritdoc />
        public string Name => "RateLimitGuard";

        /// <inheritdoc />
        public string Description => _windowType == RateLimitWindowType.Decay ? $"Limit of {Limit} with a decay rate of {_decayRate}" : $"Limit of {Limit} per {TimeSpan}";

        /// <summary>
        /// The limit per period
        /// </summary>
        public int Limit { get; }
        /// <summary>
        /// The time period for the limit
        /// </summary>
        public TimeSpan TimeSpan { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="keySelector">The rate limit key selector</param>
        /// <param name="filter">Filter for rate limit items. Only when the rate limit item passes the filter the guard will apply</param>
        /// <param name="limit">Limit per period</param>
        /// <param name="timeSpan">Timespan for the period</param>
        /// <param name="windowType">Type of rate limit window</param>
        /// <param name="decayPerTimeSpan">The decay per timespan if windowType is DecayWindowTracker</param>
        /// <param name="connectionWeight">The weight of a new connection</param>
        public RateLimitGuard(Func<RequestDefinition, string, string?, string> keySelector, IGuardFilter filter, int limit, TimeSpan timeSpan, RateLimitWindowType windowType, double? decayPerTimeSpan = null, int? connectionWeight = null)
            : this(keySelector, new[] { filter }, limit, timeSpan, windowType, decayPerTimeSpan, connectionWeight)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="keySelector">The rate limit key selector</param>
        /// <param name="filters">Filters for rate limit items. Only when the rate limit item passes all filters the guard will apply</param>
        /// <param name="limit">Limit per period</param>
        /// <param name="timeSpan">Timespan for the period</param>
        /// <param name="windowType">Type of rate limit window</param>
        /// <param name="decayPerTimeSpan">The decay per timespan if windowType is DecayWindowTracker</param>
        /// <param name="connectionWeight">The weight of a new connection</param>
        public RateLimitGuard(Func<RequestDefinition, string, string?, string> keySelector, IEnumerable<IGuardFilter> filters, int limit, TimeSpan timeSpan, RateLimitWindowType windowType, double? decayPerTimeSpan = null, int? connectionWeight = null)
        {
            _filters = filters;
            _trackers = new Dictionary<string, IWindowTracker>();
            _windowType = windowType;
            Limit = limit;
            TimeSpan = timeSpan;
            _keySelector = keySelector;
            _decayRate = decayPerTimeSpan;
            _connectionWeight = connectionWeight;
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight)
        {
            foreach(var filter in _filters)
            {
                if (!filter.Passes(type, definition, host, apiKey))
                    return LimitCheck.NotApplicable;
            }

            if (type == RateLimitItemType.Connection)
                requestWeight = _connectionWeight ?? requestWeight;

            var key = _keySelector(definition, host, apiKey);
            if (!_trackers.TryGetValue(key, out var tracker))
            {
                tracker = CreateTracker();
                _trackers.Add(key, tracker);
            }

            var delay = tracker.GetWaitTime(requestWeight);
            if (delay == default)
                return LimitCheck.NotNeeded;

            return LimitCheck.Needed(delay, Limit, TimeSpan, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight)
        {
            foreach (var filter in _filters)
            {
                if (!filter.Passes(type, definition, host, apiKey))
                    return RateLimitState.NotApplied;
            }

            if (type == RateLimitItemType.Connection)
                requestWeight = _connectionWeight ?? requestWeight;

            var key = _keySelector(definition, host, apiKey);
            var tracker = _trackers[key];
            tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(Limit, TimeSpan, tracker.Current);
        }

        /// <summary>
        /// Create a new WindowTracker
        /// </summary>
        /// <returns></returns>
        protected IWindowTracker CreateTracker()
        {
            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(Limit, TimeSpan)
                : _windowType == RateLimitWindowType.Fixed ? new FixedWindowTracker(Limit, TimeSpan)
                : _windowType == RateLimitWindowType.FixedAfterFirst ? new FixedAfterStartWindowTracker(Limit, TimeSpan) :
                new DecayWindowTracker(Limit, TimeSpan, _decayRate ?? throw new InvalidOperationException("Decay rate not provided"));
        }
    }
}
