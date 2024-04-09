using CryptoExchange.Net.RateLimiting.Trackers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Retry after guard, limit until after a certain timstamp
    /// </summary>
    public class RetryAfterGuard : IRateLimitGuard
    {
        /// <inheritdoc />
        public string Name => "RetryAfterGuard";

        private DateTime _after;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="after"></param>
        public RetryAfterGuard(DateTime after)
        {
            _after = after;
        }

        /// <inheritdoc />
        public LimitCheck Check(ILogger logger, RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            var dif = _after - DateTime.UtcNow;
            if (dif <= TimeSpan.Zero)
                return LimitCheck.NotApplicable;

            return LimitCheck.Needed(dif, default, default, default);
        }

        /// <summary>
        /// Update the 'after' time
        /// </summary>
        /// <param name="after"></param>
        public void UpdateAfter(DateTime after) => _after = after;

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, Uri url, HttpMethod? method, bool signed, SecureString? apiKey, int requestWeight)
        {
            return RateLimitState.NotApplied;
        }
    }
}
