using CryptoExchange.Net.Objects;
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
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        IRequest Create(HttpMethod method, Uri uri, int requestId);

        /// <summary>
        /// Configure the requests created by this factory
        /// </summary>
        /// <param name="requestTimeout">Request timeout to use</param>
        /// <param name="httpClient">Optional shared http client instance</param>
        /// <param name="proxy">Optional proxy to use when no http client is provided</param>
        void Configure(ApiProxy? proxy, TimeSpan requestTimeout, HttpClient? httpClient=null);
    }
}
