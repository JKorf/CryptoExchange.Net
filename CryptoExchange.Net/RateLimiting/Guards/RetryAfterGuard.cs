using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    public class RetryAfterGuard : IRateLimitGuard
    {
        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();


        private DateTime _after;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="after"></param>
        public RetryAfterGuard(DateTime after)
        {
            _after = after;
        }

        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
        {
            var dif = _after - DateTime.UtcNow;
            if (dif <= TimeSpan.Zero)
                return LimitCheck.NotApplicable;

            return LimitCheck.Needed(dif, default, default, default);
        }

        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, SecureString? apiKey, int requestWeight)
        {
            return RateLimitState.NotApplied;
        }

        /// <summary>
        /// Update the 'after' time
        /// </summary>
        /// <param name="after"></param>
        public void UpdateAfter(DateTime after) => _after = after;
    }
}
