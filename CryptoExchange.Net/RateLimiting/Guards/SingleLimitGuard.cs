using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Rate limit guard for a per endpoint limit
    /// </summary>
    public class SingleLimitGuard : IRateLimitGuard
    {
        /// <summary>
        /// Default endpoint limit
        /// </summary>
        public static Func<RequestDefinition, string?, string> Default { get; } = new Func<RequestDefinition, string?, string>((def, key) => def.Path + def.Method);

        /// <summary>
        /// Endpoint limit per API key
        /// </summary>
        public static Func<RequestDefinition, string?, string> PerApiKey { get; } = new Func<RequestDefinition, string?, string>((def, key) => def.Path + def.Method + key);

        private readonly Dictionary<string, IWindowTracker> _trackers;
        private readonly RateLimitWindowType _windowType;
        private readonly double? _decayRate;
        private readonly TimeSpan _safetyMargin;
        private readonly int _limit;
        private readonly TimeSpan _period;
        private readonly Func<RequestDefinition, string?, string> _keySelector;

        /// <inheritdoc />
        public string Name => "EndpointLimitGuard";

        /// <inheritdoc />
        public string Description => _windowType == RateLimitWindowType.Decay ? $"Endpoint limit of {_limit} with a decay rate of {_decayRate}" : $"Limit of {_limit} per {_period}";

        /// <summary>
        /// Additional time to wait after a rate limit window expires to account for latency and timing differences
        /// </summary>
        public TimeSpan SafetyMargin => _safetyMargin;

        /// <summary>
        /// ctor
        /// </summary>
        public SingleLimitGuard(
            int limit,
            TimeSpan period,
            RateLimitWindowType windowType,
            TimeSpan? safetyMargin = null,
            double? decayRate = null,
            Func<RequestDefinition, string?, string>? keySelector = null)
        {
            _limit = limit;
            _period = period;
            _windowType = windowType;
            _decayRate = decayRate;
            _safetyMargin = safetyMargin ?? WindowTrackerHelpers.GetDefaultSafetyMargin(period);
            _keySelector = keySelector ?? Default;
            _trackers = new Dictionary<string, IWindowTracker>();
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string? apiKey, int requestWeight, string? keySuffix, double allowedRateRatio)
        {
            var key = _keySelector(definition, apiKey) + keySuffix;
            if (!_trackers.TryGetValue(key, out var tracker))
            {
                tracker = CreateTracker();
                _trackers.Add(key, tracker);
            }

            var delay = tracker.GetWaitTime(requestWeight, allowedRateRatio);
            if (delay == default)
                return LimitCheck.NotNeeded(_limit, _period, tracker.Current);

            return LimitCheck.Needed(delay, _limit, _period, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string? apiKey, int requestWeight, string? keySuffix)
        {
            var key = _keySelector(definition, apiKey) + keySuffix;
            var tracker = _trackers[key];
            tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(_limit, _period, tracker.Current);
        }

        /// <summary>
        /// Create a new WindowTracker
        /// </summary>
        /// <returns></returns>
        protected IWindowTracker CreateTracker()
        {
            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(_limit, _period, _safetyMargin)
                : _windowType == RateLimitWindowType.Fixed ? new FixedWindowTracker(_limit, _period, _safetyMargin) :
                new DecayWindowTracker(_limit, _period, _decayRate ?? throw new InvalidOperationException("Decay rate not provided"));
        }

        /// <inheritdoc />
        public void Reset(RateLimitItemType type, RequestDefinition definition, string? apiKey, string? keySuffix, int? amount)
        {
            var key = _keySelector(definition, apiKey) + keySuffix;
            if (!_trackers.TryGetValue(key, out var tracker))
                return;

            tracker.Reset(amount);
        }
    }
}
