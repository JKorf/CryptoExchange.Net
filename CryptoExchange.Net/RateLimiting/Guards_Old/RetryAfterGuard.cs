//using System;
//using System.Net.Http;
//using System.Security;

//namespace CryptoExchange.Net.RateLimiting.Guards
//{
//    /// <summary>
//    /// Retry after guard, limit until after a certain timstamp
//    /// </summary>
//    public class RetryAfterGuard : IRateLimitGuard
//    {
//        /// <inheritdoc />
//        public string Name => "RetryAfterGuard";

//        /// <inheritdoc />
//        public string Description => $"Limit all [Requests] until after {_after}";

//        private DateTime _after;

//        /// <summary>
//        /// ctor
//        /// </summary>
//        /// <param name="after"></param>
//        public RetryAfterGuard(DateTime after)
//        {
//            _after = after;
//        }

//        /// <inheritdoc />
//        public LimitCheck Check(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
//        {
//            var dif = _after - DateTime.UtcNow;
//            if (dif <= TimeSpan.Zero)
//                return LimitCheck.NotApplicable;

//            return LimitCheck.Needed(dif, default, default, default);
//        }

//        /// <summary>
//        /// Update the 'after' time
//        /// </summary>
//        /// <param name="after"></param>
//        public void UpdateAfter(DateTime after) => _after = after;

//        /// <inheritdoc />
//        public RateLimitState ApplyWeight(RateLimitItemType type, string host, string path, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
//        {
//            return RateLimitState.NotApplied;
//        }
//    }
//}
