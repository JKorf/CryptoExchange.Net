using System;
using System.Collections.Generic;
using System.IO;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Options
    /// </summary>
    public class ExchangeOptions
    {

        /// <summary>
        /// The api credentials
        /// </summary>
        public ApiCredentials ApiCredentials { get; set; }

        /// <summary>
        /// The base address of the client
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// Proxy to use
        /// </summary>
        public ApiProxy Proxy { get; set; }
        
        /// <summary>
        /// The log verbosity
        /// </summary>
        public LogVerbosity LogVerbosity { get; set; } = LogVerbosity.Info;

        /// <summary>
        /// The log writers
        /// </summary>
        public List<TextWriter> LogWriters { get; set; } = new List<TextWriter> { new DebugTextWriter() };           
    }

    public class ClientOptions: ExchangeOptions
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
        /// The time the server has to respond to a request before timing out
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public T Copy<T>() where T:ClientOptions, new()
        {
            var copy = new T
            {
                BaseAddress = BaseAddress,
                LogVerbosity = LogVerbosity,
                Proxy = Proxy,
                LogWriters = LogWriters,
                RateLimiters = RateLimiters,
                RateLimitingBehaviour = RateLimitingBehaviour,
                RequestTimeout = RequestTimeout
            };

            if (ApiCredentials != null)
                copy.ApiCredentials = new ApiCredentials(ApiCredentials.Key.GetString(), ApiCredentials.Secret.GetString());

            return copy;
        }
    }

    public class SocketClientOptions: ExchangeOptions
    {
        /// <summary>
        /// Whether or not the socket should automatically reconnect when losing connection
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// Time to wait between reconnect attempts
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(5);

        public T Copy<T>() where T : SocketClientOptions, new()
        {
            var copy = new T
            {
                BaseAddress = BaseAddress,
                LogVerbosity = LogVerbosity,
                Proxy = Proxy,
                LogWriters = LogWriters,
                AutoReconnect = AutoReconnect,
                ReconnectInterval = ReconnectInterval
            };

            if (ApiCredentials != null)
                copy.ApiCredentials = new ApiCredentials(ApiCredentials.Key.GetString(), ApiCredentials.Secret.GetString());

            return copy;
        }
    }
}
