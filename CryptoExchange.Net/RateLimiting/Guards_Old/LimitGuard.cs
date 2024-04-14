//using CryptoExchange.Net.Objects;
//using CryptoExchange.Net.RateLimiting.Trackers;
//using System;

//namespace CryptoExchange.Net.RateLimiting.Guards
//{
//    /// <summary>
//    /// Limit guard
//    /// </summary>
//    public abstract class LimitGuard
//    {
//        /// <summary>
//        /// The limit
//        /// </summary>
//        public int Limit { get; }
//        /// <summary>
//        /// The time period
//        /// </summary>
//        public TimeSpan TimeSpan { get; }

//        private RateLimitWindowType _windowType;
//        private double? _decayRate;

//        /// <summary>
//        /// ctor
//        /// </summary>
//        /// <param name="limit"></param>
//        /// <param name="period"></param>
//        public LimitGuard(int limit, TimeSpan period)
//        {
//            Limit = limit;
//            TimeSpan = period;
//        }

//        /// <summary>
//        /// Create a new WindowTracker
//        /// </summary>
//        /// <returns></returns>
//        protected IWindowTracker CreateTracker()
//        {
//            return _windowType == RateLimitWindowType.Sliding ? new SlidingWindowTracker(Limit, TimeSpan) 
//                : _windowType == RateLimitWindowType.Fixed ? new FixedWindowTracker(Limit, TimeSpan):
//                new DecayWindowTracker(Limit, TimeSpan, _decayRate ?? throw new InvalidOperationException("Decay rate not provided"));
//        }

//        /// <summary>
//        /// Set the window type
//        /// </summary>
//        /// <param name="type"></param>
//        /// <param name="decayRate"></param>
//        public void SetWindowType(RateLimitWindowType type, double? decayRate = null)
//        {
//            _windowType = type;
//            _decayRate = decayRate;
//        }
//    }
//}
