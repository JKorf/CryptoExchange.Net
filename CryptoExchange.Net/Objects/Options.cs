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
    /// Base options
    /// </summary>
    public class BaseOptions
    {
        /// <summary>
        /// The minimum log level to output
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// The log writers
        /// </summary>
        public List<ILogger> LogWriters { get; set; } = new List<ILogger> { new DebugLogger() };

        /// <summary>
        /// If true, the CallResult and DataEvent objects will also include the originally received json data in the OriginalData property
        /// </summary>
        public bool OutputOriginalData { get; set; } = false;

        /// <summary>
        /// Copy the values of the def to the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="def"></param>
        public void Copy<T>(T input, T def) where T : BaseOptions
        {
            input.LogLevel = def.LogLevel;
            input.LogWriters = def.LogWriters.ToList();
            input.OutputOriginalData = def.OutputOriginalData;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"LogLevel: {LogLevel}, Writers: {LogWriters.Count}, OutputOriginalData: {OutputOriginalData}";
        }
    }

    /// <summary>
    /// Base for order book options
    /// </summary>
    public class OrderBookOptions : BaseOptions
    {
        /// <summary>
        /// Whether or not checksum validation is enabled. Default is true, disabling will ignore checksum messages.
        /// </summary>
        public bool ChecksumValidationEnabled { get; set; } = true;
    }

    public class SubClientOptions
    {
        /// <summary>
        /// The base address of the sub client
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// The api credentials used for signing requests
        /// </summary>        
        public ApiCredentials? ApiCredentials { get; set; }

        /// <summary>
        /// Copy the values of the def to the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="def"></param>
        public new void Copy<T>(T input, T def) where T : SubClientOptions
        {
            input.BaseAddress = def.BaseAddress;
            input.ApiCredentials = def.ApiCredentials?.Copy();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, Credentials: {(ApiCredentials == null ? "-" : "Set")}, BaseAddress: {BaseAddress}";
        }
    }

    /// <summary>
    /// Base client options
    /// </summary>
    public class ClientOptions : BaseOptions
    {
        /// <summary>
        /// Proxy to use when connecting
        /// </summary>
        public ApiProxy? Proxy { get; set; }

        /// <summary>
        /// Copy the values of the def to the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="def"></param>
        public new void Copy<T>(T input, T def) where T : ClientOptions
        {
            base.Copy(input, def);

            input.Proxy = def.Proxy;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, Proxy: {(Proxy == null ? "-" : Proxy.Host)}";
        }
    }

    public class RestSubClientOptions: SubClientOptions
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
        /// Copy the values of the def to the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="def"></param>
        public new void Copy<T>(T input, T def) where T : RestSubClientOptions
        {
            base.Copy(input, def);

            input.RateLimiters = def.RateLimiters.ToList();
            input.RateLimitingBehaviour = def.RateLimitingBehaviour;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, RateLimiters: {RateLimiters.Count}, RateLimitBehaviour: {RateLimitingBehaviour}";
        }
    }

    /// <summary>
    /// Base for rest client options
    /// </summary>
    public class RestClientOptions : ClientOptions
    {
        /// <summary>
        /// The time the server has to respond to a request before timing out
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Http client to use. If a HttpClient is provided in this property the RequestTimeout and Proxy options will be ignored in requests and should be set on the provided HttpClient instance
        /// </summary>
        public HttpClient? HttpClient { get; set; }

        /// <summary>
        /// Copy the values of the def to the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="def"></param>
        public new void Copy<T>(T input, T def) where T : RestClientOptions
        {
            base.Copy(input, def);
                        
            input.HttpClient = def.HttpClient;
            input.RequestTimeout = def.RequestTimeout;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, RequestTimeout: {RequestTimeout:c}, HttpClient: {(HttpClient == null ? "-": "set")}";
        }
    }

    public class SocketSubClientOptions: SubClientOptions
    {
        // TODO do we need this?
    }

    /// <summary>
    /// Base for socket client options
    /// </summary>
    public class SocketClientOptions : ClientOptions
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
        /// The maximum number of times to try to reconnect, default null will retry indefinitely 
        /// </summary>
        public int? MaxReconnectTries { get; set; }

        /// <summary>
        /// The maximum number of times to try to resubscribe after reconnecting
        /// </summary>
        public int? MaxResubscribeTries { get; set; } = 5;

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
        /// The amount of subscriptions that should be made on a single socket connection. Not all exchanges support multiple subscriptions on a single socket.
        /// Setting this to a higher number increases subscription speed because not every subscription needs to connect to the server, but having more subscriptions on a 
        /// single connection will also increase the amount of traffic on that single connection, potentially leading to issues.
        /// </summary>
        public int? SocketSubscriptionsCombineTarget { get; set; }

        /// <summary>
        /// Copy the values of the def to the input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="def"></param>
        public new void Copy<T>(T input, T def) where T : SocketClientOptions
        {
            base.Copy(input, def);

            input.AutoReconnect = def.AutoReconnect;
            input.ReconnectInterval = def.ReconnectInterval;
            input.MaxReconnectTries = def.MaxReconnectTries;
            input.MaxResubscribeTries = def.MaxResubscribeTries;
            input.MaxConcurrentResubscriptionsPerSocket = def.MaxConcurrentResubscriptionsPerSocket;
            input.SocketResponseTimeout = def.SocketResponseTimeout;
            input.SocketNoDataTimeout = def.SocketNoDataTimeout;
            input.SocketSubscriptionsCombineTarget = def.SocketSubscriptionsCombineTarget;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{base.ToString()}, AutoReconnect: {AutoReconnect}, ReconnectInterval: {ReconnectInterval}, MaxReconnectTries: {MaxReconnectTries}, MaxResubscribeTries: {MaxResubscribeTries}, MaxConcurrentResubscriptionsPerSocket: {MaxConcurrentResubscriptionsPerSocket}, SocketResponseTimeout: {SocketResponseTimeout:c}, SocketNoDataTimeout: {SocketNoDataTimeout}, SocketSubscriptionsCombineTarget: {SocketSubscriptionsCombineTarget}";
        }
    }
}
