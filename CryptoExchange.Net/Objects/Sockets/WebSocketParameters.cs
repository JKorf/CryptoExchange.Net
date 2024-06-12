using CryptoExchange.Net.RateLimiting.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// Parameters for a websocket
    /// </summary>
    public class WebSocketParameters
    {
        /// <summary>
        /// The uri to connect to
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Headers to send in the connection handshake
        /// </summary>
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Cookies to send in the connection handshake
        /// </summary>
        public IDictionary<string, string> Cookies { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The fixed time to wait between reconnect attempts, only used when `ReconnectPolicy` is set to `ReconnectPolicy.ExponentialBackoff`
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Reconnect policy
        /// </summary>
        public ReconnectPolicy ReconnectPolicy { get; set; } = ReconnectPolicy.FixedDelay;

        /// <summary>
        /// Proxy for the connection
        /// </summary>
        public ApiProxy? Proxy { get; set; }

        /// <summary>
        /// The maximum time of no data received before considering the connection lost and closting/reconnecting the socket
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Interval at which to send ping frames
        /// </summary>
        public TimeSpan? KeepAliveInterval { get; set; }

        /// <summary>
        /// The rate limiter for the socket connection
        /// </summary>
        public IRateLimitGate? RateLimiter { get; set; }
        /// <summary>
        /// What to do when rate limit is reached
        /// </summary>
        public RateLimitingBehaviour RateLimitingBehaviour { get; set; }

        /// <summary>
        /// Encoding for sending/receiving data
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="uri">Uri</param>
        /// <param name="policy">Reconnect policy</param>
        public WebSocketParameters(Uri uri, ReconnectPolicy policy)
        {
            Uri = uri;
            ReconnectPolicy = policy;
        }
    }
}
