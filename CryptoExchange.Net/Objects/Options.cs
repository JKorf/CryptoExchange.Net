using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Client options
    /// </summary>
    public abstract class ClientOptions
    {
        internal event Action? OnLoggingChanged;

        private LogLevel _logLevel = LogLevel.Information;
        /// <summary>
        /// The minimum log level to output
        /// </summary>
        public LogLevel LogLevel
        {
            get => _logLevel;
            set
            {
                _logLevel = value;
                OnLoggingChanged?.Invoke();
            }
        }

        private List<ILogger> _logWriters = new List<ILogger> { new DebugLogger() };
        /// <summary>
        /// The log writers
        /// </summary>
        public List<ILogger> LogWriters
        {
            get => _logWriters;
            set
            {
                _logWriters = value;
                OnLoggingChanged?.Invoke();
            }
        }

        /// <summary>
        /// Proxy to use when connecting
        /// </summary>
        public ApiProxy? Proxy { get; set; }

        /// <summary>
        /// The api credentials used for signing requests to this API.
        /// </summary>        
        public ApiCredentials? ApiCredentials { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public ClientOptions()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="clientOptions">Copy values for the provided options</param>
        public ClientOptions(ClientOptions? clientOptions)
        {
            if (clientOptions == null)
                return;

            LogLevel = clientOptions.LogLevel;
            LogWriters = clientOptions.LogWriters.ToList();
            Proxy = clientOptions.Proxy;
            ApiCredentials = clientOptions.ApiCredentials?.Copy();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseOptions">Copy values for the provided options</param>
        /// <param name="newValues">Copy values for the provided options</param>
        internal ClientOptions(ClientOptions baseOptions, ClientOptions? newValues)
        {
            Proxy = newValues?.Proxy ?? baseOptions.Proxy;
            LogLevel = baseOptions.LogLevel;
            LogWriters = baseOptions.LogWriters.ToList();
            ApiCredentials = newValues?.ApiCredentials?.Copy() ?? baseOptions.ApiCredentials?.Copy();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"LogLevel: {LogLevel}, Writers: {LogWriters.Count}, Proxy: {(Proxy == null ? "-" : Proxy.Host)}";
        }
    }

    /// <summary>
    /// API client options
    /// </summary>
    public class ApiClientOptions
    {
        /// <summary>
        /// If true, the CallResult and DataEvent objects will also include the originally received json data in the OriginalData property
        /// </summary>
        public bool OutputOriginalData { get; set; } = false;

        /// <summary>
        /// The base address of the API
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// The api credentials used for signing requests to this API. Overrides API credentials provided in the client options
        /// </summary>        
        public ApiCredentials? ApiCredentials { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
#pragma warning disable 8618 // Will always get filled by the implementation
        public ApiClientOptions()
        {
        }
#pragma warning restore 8618

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseAddress">Base address for the API</param>
        public ApiClientOptions(string baseAddress)
        {
            BaseAddress = baseAddress;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseOptions">Copy values for the provided options</param>
        /// <param name="newValues">Copy values for the provided options</param>
        public ApiClientOptions(ApiClientOptions baseOptions, ApiClientOptions? newValues)
        {
            BaseAddress = newValues?.BaseAddress ?? baseOptions.BaseAddress;
            ApiCredentials = newValues?.ApiCredentials?.Copy() ?? baseOptions.ApiCredentials?.Copy();
            OutputOriginalData = newValues?.OutputOriginalData ?? baseOptions.OutputOriginalData;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"OutputOriginalData: {OutputOriginalData}, Credentials: {(ApiCredentials == null ? "-" : "Set")}, BaseAddress: {BaseAddress}";
        }
    }
    
    /// <summary>
    /// Rest API client options
    /// </summary>
    public class RestApiClientOptions: ApiClientOptions
    {
        /// <summary>
        /// The time the server has to respond to a request before timing out
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Http client to use. If a HttpClient is provided in this property the RequestTimeout and Proxy options provided in these options will be ignored in requests and should be set on the provided HttpClient instance
        /// </summary>
        public HttpClient? HttpClient { get; set; }

        /// <summary>
        /// List of rate limiters to use
        /// </summary>
        public List<IRateLimiter> RateLimiters { get; set; } = new List<IRateLimiter>();

        /// <summary>
        /// What to do when a call would exceed the rate limit
        /// </summary>
        public RateLimitingBehaviour RateLimitingBehaviour { get; set; } = RateLimitingBehaviour.Wait;

        /// <summary>
        /// Whether or not to automatically sync the local time with the server time
        /// </summary>
        public bool AutoTimestamp { get; set; }

        /// <summary>
        /// How often the timestamp adjustment between client and server is recalculated. If you need a very small TimeSpan here you're probably better of syncing your server time more often
        /// </summary>
        public TimeSpan TimestampRecalculationInterval { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// ctor
        /// </summary>
        public RestApiClientOptions()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseAddress">Base address for the API</param>
        public RestApiClientOptions(string baseAddress): base(baseAddress)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseOn">Copy values for the provided options</param>
        /// <param name="newValues">Copy values for the provided options</param>
        public RestApiClientOptions(RestApiClientOptions baseOn, RestApiClientOptions? newValues): base(baseOn, newValues)
        {
            HttpClient = newValues?.HttpClient ?? baseOn.HttpClient;
            RequestTimeout = newValues == default ? baseOn.RequestTimeout : newValues.RequestTimeout;
            RateLimitingBehaviour = newValues?.RateLimitingBehaviour ?? baseOn.RateLimitingBehaviour;
            AutoTimestamp = newValues?.AutoTimestamp ?? baseOn.AutoTimestamp;
            TimestampRecalculationInterval = newValues?.TimestampRecalculationInterval ?? baseOn.TimestampRecalculationInterval;
            RateLimiters = newValues?.RateLimiters.ToList() ?? baseOn?.RateLimiters.ToList() ?? new List<IRateLimiter>();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, RequestTimeout: {RequestTimeout:c}, HttpClient: {(HttpClient == null ? "-" : "set")}, RateLimiters: {RateLimiters?.Count}, RateLimitBehaviour: {RateLimitingBehaviour}, AutoTimestamp: {AutoTimestamp}, TimestampRecalculationInterval: {TimestampRecalculationInterval}";
        }
    }

    /// <summary>
    /// Rest API client options
    /// </summary>
    public class SocketApiClientOptions : ApiClientOptions
    {
        /// <summary>
        /// Whether or not the socket should automatically reconnect when losing connection
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// Time to wait between reconnect attempts
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Max number of concurrent resubscription tasks per socket after reconnecting a socket
        /// </summary>
        public int MaxConcurrentResubscriptionsPerSocket { get; set; } = 5;

        /// <summary>
        /// The max time to wait for a response after sending a request on the socket before giving a timeout
        /// </summary>
        public TimeSpan SocketResponseTimeout { get; set; } = TimeSpan.FromSeconds(10);

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
        /// ctor
        /// </summary>
        public SocketApiClientOptions()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseAddress">Base address for the API</param>
        public SocketApiClientOptions(string baseAddress) : base(baseAddress)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseOptions">Copy values for the provided options</param>
        /// <param name="newValues">Copy values for the provided options</param>
        public SocketApiClientOptions(SocketApiClientOptions baseOptions, SocketApiClientOptions? newValues) : base(baseOptions, newValues)
        {
            if (baseOptions == null)
                return;

            AutoReconnect = baseOptions.AutoReconnect;
            ReconnectInterval = baseOptions.ReconnectInterval;
            MaxConcurrentResubscriptionsPerSocket = baseOptions.MaxConcurrentResubscriptionsPerSocket;
            SocketResponseTimeout = baseOptions.SocketResponseTimeout;
            SocketNoDataTimeout = baseOptions.SocketNoDataTimeout;
            SocketSubscriptionsCombineTarget = baseOptions.SocketSubscriptionsCombineTarget;
            MaxSocketConnections = baseOptions.MaxSocketConnections;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, AutoReconnect: {AutoReconnect}, ReconnectInterval: {ReconnectInterval}, MaxConcurrentResubscriptionsPerSocket: {MaxConcurrentResubscriptionsPerSocket}, SocketResponseTimeout: {SocketResponseTimeout:c}, SocketNoDataTimeout: {SocketNoDataTimeout}, SocketSubscriptionsCombineTarget: {SocketSubscriptionsCombineTarget}, MaxSocketConnections: {MaxSocketConnections}";
        }
    }

    /// <summary>
    /// Base for order book options
    /// </summary>
    public class OrderBookOptions : ClientOptions
    {
        /// <summary>
        /// Whether or not checksum validation is enabled. Default is true, disabling will ignore checksum messages.
        /// </summary>
        public bool ChecksumValidationEnabled { get; set; } = true;
    }

}
