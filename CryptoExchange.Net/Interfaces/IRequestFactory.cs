using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using System;
using System.Net.Http;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Request factory interface
    /// </summary>
    public interface IRequestFactory
    {
        /// <summary>
        /// Create a request for an uri
        /// </summary>
        IRequest Create(Version httpRequestVersion, HttpMethod method, Uri uri, int requestId);

        /// <summary>
        /// Configure the requests created by this factory
        /// </summary>
        /// <param name="options">Rest client options</param>
        /// <param name="httpClient">Optional shared http client instance</param>
        void Configure(RestExchangeOptions options, HttpClient? httpClient = null);

        /// <summary>
        /// Update settings
        /// </summary>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="requestTimeout">Request timeout to use</param>
        /// <param name="httpKeepAliveInterval">Http client keep alive interval</param>
        void UpdateSettings(ApiProxy? proxy, TimeSpan requestTimeout, TimeSpan? httpKeepAliveInterval);
    }
}
