using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.Objects.Options
{
    /// <summary>
    /// Options to update
    /// </summary>
    public class UpdateOptions
    {
        /// <summary>
        /// Proxy setting. Note that if this is not provided any previously set proxy will be reset
        /// </summary>
        public ApiProxy? Proxy { get; set; }
        /// <summary>
        /// Api credentials
        /// </summary>
        public ApiCredentials? ApiCredentials { get; set; }
        /// <summary>
        /// Request timeout
        /// </summary>
        public TimeSpan? RequestTimeout { get; set; }
    }
}
