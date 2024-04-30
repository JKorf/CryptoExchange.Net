using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;
using System.Security;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Rate limit guard for a per endpoint limit
    /// </summary>
    public class SingleLimitGuard : IRateLimitGuard
    {
        private readonly Dictionary<string, IWindowTracker> _trackers;
        private readonly RateLimitWindowType _windowType;
        private readonly double? _decayRate;

        /// <inheritdoc />
        public string Name => "EndpointLimitGuard";

        /// <inheritdoc />
        public string Description => $"Limit requests to endpoint";

        /// <summary>
        /// ctor
        /// </summary>
        public SingleLimitGuard(RateLimitWindowType windowType, double? decayRate = null)
        {
            _windowType = windowType;
            _decayRate = decayRate;
            _trackers = new Dictionary<string, IWindowTracker>();
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
        {
            var key = definition.Path + definition.Method;
            if (!_trackers.TryGetValue(key, out var tracker))
            {
                tracker = CreateTracker(definition.EndpointLimitCount!.Value, definition.EndpointLimitPeriod!.Value);
                _trackers.Add(key, tracker);
            }

            var delay = tracker.GetWaitTime(requestWeight);
            if (delay == default)
                return LimitCheck.NotNeeded;

            return LimitCheck.Needed(delay, definition.EndpointLimitCount!.Value, definition.EndpointLimitPeriod!.Value, tracker.Current);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
        {
            var key = definition.Path + definition.Method;
            var tracker = _trackers[key];
            tracker.ApplyWeight(requestWeight);
            return RateLimitState.Applied(definition.EndpointLimitCount!.Value, definition.EndpointLimitPeriod!.Value, tracker.Current);
        }

        /// <summary>
        /// Create a new WindowTracker
        /// </summary>
        /// <returns></returns>
        protected IWindowTracker CreateTracker(int limit, TimeSpan timeSpan)
        {
            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(limit, timeSpan)
                : _windowType == RateLimitWindowType.Fixed ? new FixedWindowTracker(limit, timeSpan) :
                new DecayWindowTracker(limit, timeSpan, _decayRate ?? throw new InvalidOperationException("Decay rate not provided"));
        }
    }
}
