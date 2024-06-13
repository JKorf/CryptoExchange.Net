using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Options for a rest exchange client
    /// </summary>
    public class RestExchangeOptions: ExchangeOptions
    {
        /// <summary>
        /// Whether or not to automatically sync the local time with the server time
        /// </summary>
        public bool AutoTimestamp { get; set; }

        /// <summary>
        /// How often the timestamp adjustment between client and server is recalculated. If you need a very small TimeSpan here you're probably better of syncing your server time more often
        /// </summary>
        public TimeSpan TimestampRecalculationInterval { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Whether caching is enabled. Caching will only be applied to GET http requests. The lifetime of cached results can be determined by the `CachingMaxAge` option
        /// </summary>
        public bool CachingEnabled { get; set; } = false;

        /// <summary>
        /// The max age of a cached entry, only used when the `CachingEnabled` options is set to true. When a cached entry is older than the max age it will be discarded and a new server request will be done
        /// </summary>
        public TimeSpan CachingMaxAge { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Create a copy of this options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Copy<T>() where T : RestExchangeOptions, new()
        {
            return new T
            {
                OutputOriginalData = OutputOriginalData,
                AutoTimestamp = AutoTimestamp,
                TimestampRecalculationInterval = TimestampRecalculationInterval,
                ApiCredentials = ApiCredentials?.Copy(),
                Proxy = Proxy,
                RequestTimeout = RequestTimeout,
                RateLimiterEnabled = RateLimiterEnabled,
                RateLimitingBehaviour = RateLimitingBehaviour,
                CachingEnabled = CachingEnabled,
                CachingMaxAge = CachingMaxAge,
            };
        }
    }

    /// <summary>
    /// Options for a rest exchange client
    /// </summary>
    /// <typeparam name="TEnvironment"></typeparam>
    public class RestExchangeOptions<TEnvironment> : RestExchangeOptions where TEnvironment : TradeEnvironment
    {
        /// <summary>
        /// Trade environment. Contains info about URL's to use to connect to the API. To swap environment select another environment for
        /// the exhange's environment list or create a custom environment using either `[Exchange]Environment.CreateCustom()` or `[Exchange]Environment.[Environment]`, for example `KucoinEnvironment.TestNet` or `BinanceEnvironment.Live`
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TEnvironment Environment { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Create a copy of this options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public new T Copy<T>() where T : RestExchangeOptions<TEnvironment>, new()
        {
            var result = base.Copy<T>();
            result.Environment = Environment;
            return result;
        }
    }

    /// <summary>
    /// Options for a rest exchange client
    /// </summary>
    /// <typeparam name="TEnvironment"></typeparam>
    /// <typeparam name="TApiCredentials"></typeparam>
    public class RestExchangeOptions<TEnvironment, TApiCredentials> : RestExchangeOptions<TEnvironment> where TEnvironment : TradeEnvironment where TApiCredentials : ApiCredentials
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
