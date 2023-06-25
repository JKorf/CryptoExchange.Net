using System;
using System.Net;
using System.Net.Http;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// Request factory
    /// </summary>
    public class RequestFactory : IRequestFactory
    {
        private HttpClient? _httpClient;        

        /// <inheritdoc />
        public void Configure(TimeSpan requestTimeout, HttpClient? client = null)
        {
            _httpClient = client ?? new HttpClient()
            {
                Timeout = requestTimeout
            };
        }

        /// <inheritdoc />
        public IRequest Create(HttpMethod method, Uri uri, int requestId)
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Cant create request before configuring http client");

            return new Request(new HttpRequestMessage(method, uri), _httpClient, requestId);
        }
    }
}
