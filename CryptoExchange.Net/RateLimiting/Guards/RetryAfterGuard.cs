using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace CryptoExchange.Net.RateLimiting.Guards
{
    /// <summary>
    /// Retry after guard
    /// </summary>
    public class RetryAfterGuard : IRateLimitGuard
    {
        /// <summary>
        /// Additional wait time to apply to account for time offset between server and client
        /// </summary>
        private static readonly TimeSpan _windowBuffer = TimeSpan.FromMilliseconds(1000);

        /// <inheritdoc />
        public string Name => "RetryAfterGuard";

        /// <inheritdoc />
        public string Description => $"Pause requests until after {After}";

        /// <summary>
        /// The timestamp after which requests are allowed again
        /// </summary>
        public DateTime After { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="after"></param>
        public RetryAfterGuard(DateTime after)
        {
            After = after;
        }

        /// <inheritdoc />
        public LimitCheck Check(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight)
        {
            var dif = (After + _windowBuffer) - DateTime.UtcNow;
            if (dif <= TimeSpan.Zero)
                return LimitCheck.NotApplicable;

            return LimitCheck.Needed(dif, default, default, default);
        }

        /// <inheritdoc />
        public RateLimitState ApplyWeight(RateLimitItemType type, RequestDefinition definition, string host, string? apiKey, int requestWeight)
        {
            return RateLimitState.NotApplied;
        }

        /// <summary>
        /// Update the 'after' time
        /// </summary>
        /// <param name="after"></param>
        public void UpdateAfter(DateTime after) => After = after;
    }
}
