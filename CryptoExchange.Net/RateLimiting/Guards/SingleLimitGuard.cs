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
        public static Func<RequestDefinition, string, string?, string> Default { get; } = new Func<RequestDefinition, string, string?, string>((def, host, key) => def.Path + def.Method);

        /// <summary>
        /// Endpoint limit per API key
        /// </summary>
        public static Func<RequestDefinition, string, string?, string> PerApiKey { get; } = new Func<RequestDefinition, string, string?, string>((def, host, key) => def.Path + def.Method);

        private readonly Dictionary<string, IWindowTracker> _trackers;
        private readonly RateLimitWindowType _windowType;
        private readonly double? _decayRate;
        private readonly int _limit;
        private readonly TimeSpan _period;
        private readonly Func<RequestDefinition, string, string?, string> _keySelector;

        /// <inheritdoc />
        public string Name => "EndpointLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit requests to endpoint";

        /// <summary>
        /// ctor
        /// </summary>
        public SingleLimitGuard(
            int limit,
            TimeSpan period,
            RateLimitWindowType windowType,
            double? decayRate = null,
            Func<RequestDefinition, string, string?, string>? keySelector = null)
        {
            _limit = limit;
            _period = period;
            _windowType = windowType;
            _decayRate = decayRate;
            _keySelector = keySelector ?? Default;
            _trackers = new Dictionary<string, IWindowTracker>();
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight)
        {
            var key = _keySelector(definition, host, apiKey);
            if (!_trackers.TryGetValue(key, out var tracker))
            {
                tracker = CreateTracker();
                _trackers.Add(key, tracker);
            }

            var delay = tracker.GetWaitTime(requestWeight);
            if (delay == default)
                return LimitCheck.NotNeeded;

            return LimitCheck.Needed(delay, _limit, _period, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight)
        {
            var key = _keySelector(definition, host, apiKey);
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
            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(_limit, _period)
                : _windowType == RateLimitWindowType.Fixed ? new FixedWindowTracker(_limit, _period) :
                new DecayWindowTracker(_limit, _period, _decayRate ?? throw new InvalidOperationException("Decay rate not provided"));
        }
    }
}
