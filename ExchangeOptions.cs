using System.Collections.Generic;
using System.IO;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.RateLimiter;

namespace CryptoExchange.Net
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
        /// Proxy to use
        /// </summary>
        public ApiProxy Proxy { get; set; }
        
        /// <summary>
        /// The log verbosity
        /// </summary>
        public LogVerbosity LogVerbosity { get; set; } = LogVerbosity.Info;

        /// <summary>
        /// The log writer
        /// </summary>
        public TextWriter LogWriter { get; set; } = new DebugTextWriter();

        /// <summary>
        /// List of ratelimiters to use
        /// </summary>
        public List<IRateLimiter> RateLimiters { get; set; } = new List<IRateLimiter>();
    }
}
