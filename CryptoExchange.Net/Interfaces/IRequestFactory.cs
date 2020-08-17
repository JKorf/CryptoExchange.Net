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
        /// <returns></returns>
        IRequest Create(HttpMethod method, string uri);

        /// <summary>
        /// Configure the requests created by this factory
        /// </summary>
        /// <param name="requestTimeout">Request timeout to use</param>
        /// <param name="proxy">Proxy settings to use</param>       
        void Configure(TimeSpan requestTimeout, ApiProxy? proxy);
    }
}
