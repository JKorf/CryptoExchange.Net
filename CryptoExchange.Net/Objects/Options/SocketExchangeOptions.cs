using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Options for a websocket exchange client
    /// </summary>
    public class SocketExchangeOptions : ExchangeOptions
    {
        /// <summary>
        /// The fixed time to wait between reconnect attempts, only used when `ReconnectPolicy` is set to `ReconnectPolicy.ExponentialBackoff`
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Reconnect policy
        /// </summary>
        public ReconnectPolicy ReconnectPolicy { get; set; } = ReconnectPolicy.FixedDelay;

        /// <summary>
        /// Max number of concurrent resubscription tasks per socket after reconnecting a socket
        /// </summary>
        public int MaxConcurrentResubscriptionsPerSocket { get; set; } = 5;

        /// <summary>
        /// The max time of not receiving any data after which the connection is assumed to be dropped. This can only be used for socket connections where a steady flow of data is expected,
        /// for example when the server sends intermittent ping requests
        /// </summary>
        public TimeSpan SocketNoDataTimeout { get; set; }

        /// <summary>
        /// The amount of subscriptions that should be made on a single socket connection. Not all API's support multiple subscriptions on a single socket.
        /// Setting this to a higher number increases subscription speed because not every subscription needs to connect to the server, but having more subscriptions on a 
        /// single connection will also increase the amount of traffic on that single connection, potentially leading to issues.
        /// </summary>
        public int? SocketSubscriptionsCombineTarget { get; set; }

        /// <summary>
        /// The max amount of connections to make to the server. Can be used for API's which only allow a certain number of connections. Changing this to a high value might cause issues.
        /// </summary>
        public int? MaxSocketConnections { get; set; }

        /// <summary>
        /// The time to wait after connecting a socket before sending messages. Can be used for API's which will rate limit if you subscribe directly after connecting.
        /// </summary>
        public TimeSpan DelayAfterConnect { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// This delay is used to set a RetryAfter guard on the connection after a rate limit is hit on the server. 
        /// This is used to prevent the client from reconnecting too quickly after a rate limit is hit.
        /// </summary>
        public TimeSpan? ConnectDelayAfterRateLimited { get; set; }

        /// <summary>
        /// Create a copy of this options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Copy<T>() where T : SocketExchangeOptions, new()
        {
            return new T
            {
                ApiCredentials = ApiCredentials?.Copy(),
                OutputOriginalData = OutputOriginalData,
                ReconnectPolicy = ReconnectPolicy,
                DelayAfterConnect = DelayAfterConnect,
                MaxConcurrentResubscriptionsPerSocket = MaxConcurrentResubscriptionsPerSocket,
                ReconnectInterval = ReconnectInterval,
                SocketNoDataTimeout = SocketNoDataTimeout,
                SocketSubscriptionsCombineTarget = SocketSubscriptionsCombineTarget,
                MaxSocketConnections = MaxSocketConnections,
                Proxy = Proxy,
                RequestTimeout = RequestTimeout,
                RateLimitingBehaviour = RateLimitingBehaviour,
                RateLimiterEnabled = RateLimiterEnabled,
            };
        }
    }

    /// <summary>
    /// Options for a socket exchange client
    /// </summary>
    /// <typeparam name="TEnvironment"></typeparam>
    public class SocketExchangeOptions<TEnvironment> : SocketExchangeOptions where TEnvironment : TradeEnvironment
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
        public new T Copy<T>() where T : SocketExchangeOptions<TEnvironment>, new()
        {
            var result = base.Copy<T>();
            result.Environment = Environment;
            return result;
        }
    }

    /// <summary>
    /// Options for a socket exchange client
    /// </summary>
    /// <typeparam name="TEnvironment"></typeparam>
    /// <typeparam name="TApiCredentials"></typeparam>
    public class SocketExchangeOptions<TEnvironment, TApiCredentials> : SocketExchangeOptions<TEnvironment> where TEnvironment : TradeEnvironment where TApiCredentials : ApiCredentials
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
