using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    public class SingleLimitGuard : IRateLimitGuard
    {
        private readonly Dictionary<string, IWindowTracker> _trackers;
        private RateLimitWindowType _windowType;
        private double? _decayRate;

        public string Name => "";

        public string Description => "";

        public SingleLimitGuard()
        {
            _trackers = new Dictionary<string, IWindowTracker>();
        }

        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
        {
            var key = definition.Path + definition.Method + definition;
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

        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
        {
            var key = definition.Path + definition.Method + definition;
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
