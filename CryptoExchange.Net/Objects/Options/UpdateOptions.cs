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
        /// Request timeout
        /// </summary>
        public TimeSpan? RequestTimeout { get; set; }
    }

    /// <summary>
    /// Options to update
    /// </summary>
    public class UpdateOptions<TApiCredentials>: UpdateOptions
        where TApiCredentials : ApiCredentials
    {
        /// <summary>
        /// Api credentials
        /// </summary>
        public TApiCredentials? ApiCredentials { get; set; }
    }
}
