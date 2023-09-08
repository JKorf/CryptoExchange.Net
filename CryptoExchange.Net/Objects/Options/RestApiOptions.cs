using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Http api options
    /// </summary>
    public class RestApiOptions : ApiOptions
    {
        /// <summary>
        /// List of rate limiters to use
        /// </summary>
        public List<IRateLimiter> RateLimiters { get; set; } = new List<IRateLimiter>();

        /// <summary>
        /// What to do when a call would exceed the rate limit
        /// </summary>
        public RateLimitingBehaviour RateLimitingBehaviour { get; set; } = RateLimitingBehaviour.Wait;

        /// <summary>
        /// Whether or not any further changes of RateLimiters or RateLimitingBehaviour impact RestApiClient behaviour
        /// </summary>
        public bool IsRateLimitReplacingAllowed { get; set; } = false;

        /// <summary>
        /// Whether or not to automatically sync the local time with the server time
        /// </summary>
        public bool? AutoTimestamp { get; set; }

        /// <summary>
        /// How often the timestamp adjustment between client and server is recalculated. If you need a very small TimeSpan here you're probably better of syncing your server time more often
        /// </summary>
        public TimeSpan? TimestampRecalculationInterval { get; set; }

        /// <summary>
        /// Create a copy of this options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T Copy<T>() where T : RestApiOptions, new()
        {
            return new T
            {
                ApiCredentials = ApiCredentials?.Copy(),
                OutputOriginalData = OutputOriginalData,
                AutoTimestamp = AutoTimestamp,
                RateLimiters = RateLimiters,
                RateLimitingBehaviour = RateLimitingBehaviour,
                IsRateLimitReplacingAllowed = IsRateLimitReplacingAllowed,
                TimestampRecalculationInterval = TimestampRecalculationInterval
            };
        }
    }

    /// <summary>
    /// Http API options
    /// </summary>
    /// <typeparam name="TApiCredentials"></typeparam>
    public class RestApiOptions<TApiCredentials>: RestApiOptions where TApiCredentials: ApiCredentials
    {
        /// <summary>
        /// The api credentials used for signing requests to this API.
        /// </summary>
        public new TApiCredentials? ApiCredentials
        {
            get => (TApiCredentials?)base.ApiCredentials;
            set => base.ApiCredentials = value;
        }
    }
}
