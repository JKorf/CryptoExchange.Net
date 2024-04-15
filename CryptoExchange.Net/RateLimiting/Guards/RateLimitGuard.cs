using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Trackers;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    public class RateLimitGuard : IRateLimitGuard
    {
        private readonly IEnumerable<IGuardFilter> _filters;
        private readonly Dictionary<string, IWindowTracker> _trackers;
        private RateLimitWindowType _windowType;
        private double? _decayRate;
        private int? _connectionWeight;
        private readonly Func<RequestDefinition, string, SecureString?, string> _keySelector;

        public string Name => "RateLimitGuard";

        public string Description => _windowType == RateLimitWindowType.Decay ? $"Limit of {Limit} with a decay rate of {_decayRate}" : $"Limit of {Limit} per {TimeSpan}";

        /// <summary>
        /// The limit
        /// </summary>
        public int Limit { get; }
        /// <summary>
        /// The time period
        /// </summary>
        public TimeSpan TimeSpan { get; }

        public RateLimitGuard(Func<RequestDefinition, string, SecureString?, string> keySelector, IGuardFilter filters, int limit, TimeSpan timeSpan, RateLimitWindowType windowType, double? decayPerTimeSpan = null, int? connectionWeight = null)
            : this(keySelector, new[] { filters }, limit, timeSpan, windowType, decayPerTimeSpan, connectionWeight)
        {
        }

        public RateLimitGuard(Func<RequestDefinition, string, SecureString?, string> keySelector, IEnumerable<IGuardFilter> filters, int limit, TimeSpan timeSpan, RateLimitWindowType windowType, double? decayPerTimeSpan = null, int? connectionWeight = null)
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

        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
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

        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
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
