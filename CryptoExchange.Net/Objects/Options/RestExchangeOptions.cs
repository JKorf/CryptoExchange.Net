using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Options for a rest exchange client
    /// </summary>
    public class RestExchangeOptions : ExchangeOptions
    {

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
        /// The HTTP protocol version to use, typically 2.0 or 1.1
        /// </summary>
        public Version HttpVersion { get; set; }
#if NET5_0_OR_GREATER
            = new Version(2, 0);
#else
            = new Version(1, 1);
#endif

        /// <summary>
        /// Http client keep alive interval for keeping connections open. Only applied when using dotnet8.0 or higher and dependency injection
        /// </summary>
        public TimeSpan? HttpKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);
#if NET5_0_OR_GREATER
        /// <summary>
        /// Enable multiple simultaneous HTTP 2 connections. Only applied when using dependency injection
        /// </summary>
        public bool HttpEnableMultipleHttp2Connections { get; set; } = false;
        /// <summary>
        /// Lifetime of pooled HTTP connections; the time before a connection is recreated. Only applied when using dependency injection
        /// </summary>
        public TimeSpan HttpPooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(15);
        /// <summary>
        /// Idle timeout of pooled HTTP connections; the time before an open connection is closed when there are no requests. Only applied when using dependency injection
        /// </summary>
        public TimeSpan HttpPooledConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(2);
        /// <summary>
        /// Max number of connections per server. Only applied when using dependency injection
        /// </summary>
        public int HttpMaxConnectionsPerServer { get; set; } = int.MaxValue;
#endif 

        /// <summary>
        /// Set the values of this options on the target options
        /// </summary>
        public T Set<T>(T item) where T : RestExchangeOptions, new()
        {
            item.OutputOriginalData = OutputOriginalData;
            item.AutoTimestamp = AutoTimestamp;
            item.TimestampRecalculationInterval = TimestampRecalculationInterval;
            item.Proxy = Proxy;
            item.RequestTimeout = RequestTimeout;
            item.RateLimiterEnabled = RateLimiterEnabled;
            item.RateLimitingBehaviour = RateLimitingBehaviour;
            item.CachingEnabled = CachingEnabled;
            item.CachingMaxAge = CachingMaxAge;
            item.HttpVersion = HttpVersion;
            item.HttpKeepAliveInterval = HttpKeepAliveInterval;
#if NET5_0_OR_GREATER
            item.HttpMaxConnectionsPerServer = HttpMaxConnectionsPerServer;
            item.HttpPooledConnectionLifetime = HttpPooledConnectionLifetime;
            item.HttpPooledConnectionIdleTimeout = HttpPooledConnectionIdleTimeout;
            item.HttpEnableMultipleHttp2Connections = HttpEnableMultipleHttp2Connections;
#endif
            return item;
        }
    }

    /// <inheritdoc />
    public class RestExchangeOptions<TEnvironment> : RestExchangeOptions
        where TEnvironment : TradeEnvironment
    {
        /// <summary>
        /// Trade environment. Contains info about URL's to use to connect to the API. To swap environment select another environment for
        /// the exchange's environment list or create a custom environment using either `[Exchange]Environment.CreateCustom()` or `[Exchange]Environment.[Environment]`, for example `KucoinEnvironment.TestNet` or `BinanceEnvironment.Live`
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TEnvironment Environment { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Set the values of this options on the target options
        /// </summary>
        public new T Set<T>(T target) where T : RestExchangeOptions<TEnvironment>, new()
        {
            base.Set(target);
            target.Environment = Environment;
            return target;
        }
    }

    /// <inheritdoc />
    public class RestExchangeOptions<TEnvironment, TApiCredentials> : RestExchangeOptions<TEnvironment>
        where TEnvironment : TradeEnvironment
        where TApiCredentials : ApiCredentials
    {

        /// <summary>
        /// The api credentials used for signing requests to this API.
        /// </summary>        
        public TApiCredentials? ApiCredentials { get; set; }

        /// <summary>
        /// Set the values of this options on the target options
        /// </summary>
        public new T Set<T>(T item) where T : RestExchangeOptions<TEnvironment, TApiCredentials>, new()
        {
            base.Set(item);
            item.ApiCredentials = (TApiCredentials?)ApiCredentials?.Copy();
            return item;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, ApiCredentials: {(ApiCredentials == null ? "-" : "set")}";
        }

    }
}
