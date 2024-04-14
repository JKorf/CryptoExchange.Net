//using CryptoExchange.Net.Objects;
//using CryptoExchange.Net.RateLimiting.Trackers;
//using System;
//using System.Collections.Generic;

//namespace CryptoExchange.Net.RateLimiting.Guards
//{
//    /// <summary>
//    /// Individual limit guard, limit the requests to 1 specific endpoint
//    /// </summary>
//    public class EndpointIndividualLimitGuard
//    {
//        private readonly Dictionary<string, IWindowTracker> _trackers;
//        private readonly RateLimitWindowType _type;

//        /// <summary>
//        /// ctor
//        /// </summary>
//        public EndpointIndividualLimitGuard(RateLimitWindowType type)
//        {
//            _trackers = new Dictionary<string, IWindowTracker>();
//            _type = type;
//        }

//        /// <inheritdoc />
//        public LimitCheck Check(string key, int limit, TimeSpan period, int requestWeight)
//        {
//            if (!_trackers.TryGetValue(key, out var tracker))
//            {
//                tracker = _type == RateLimitWindowType.Sliding? new SlidingWindowTracker(limit, period) : new FixedWindowTracker(limit, period);
//                _trackers[key] = tracker;
//            }

//            var delay = tracker.GetWaitTime(requestWeight);
//            return delay == default ? LimitCheck.NotNeeded : LimitCheck.Needed(delay, tracker.Limit, tracker.TimePeriod, tracker.Current);
//        }

//        /// <inheritdoc />
//        public RateLimitState ApplyWeight(string key, int weight)
//        {
//            var tracker = _trackers[key];
//            tracker.ApplyWeight(weight);
//            return RateLimitState.Applied(tracker.Limit, tracker.TimePeriod, tracker.Current);
//        }        
//    }
//}
