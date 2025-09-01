using System;
using System.Net;
using System.Net.Http;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// Request factory
    /// </summary>
    public class RequestFactory : IRequestFactory
    {
        private HttpClient? _httpClient;

        /// <inheritdoc />
        public void Configure(RestExchangeOptions options, HttpClient? client = null)
        {
            if (client == null)
                client = CreateClient(options.Proxy, options.RequestTimeout, options.HttpKeepAliveInterval);

            _httpClient = client;
        }

        /// <inheritdoc />
        public IRequest Create(Version httpRequestVersion, HttpMethod method, Uri uri, int requestId)
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Cant create request before configuring http client");

            var requestMessage = new HttpRequestMessage(method, uri);
            requestMessage.Version = httpRequestVersion;
#if NET5_0_OR_GREATER
            requestMessage.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
#endif
            return new Request(requestMessage, _httpClient, requestId);
        }

        /// <inheritdoc />
        public void UpdateSettings(ApiProxy? proxy, TimeSpan requestTimeout, TimeSpan? httpKeepAliveInterval)
        {
            _httpClient = CreateClient(proxy, requestTimeout, httpKeepAliveInterval);
        }

        private static HttpClient CreateClient(ApiProxy? proxy, TimeSpan requestTimeout, TimeSpan? httpKeepAliveInterval)
        {
            var handler = LibraryHelpers.CreateHttpClientMessageHandler(proxy, httpKeepAliveInterval);
            var client = new HttpClient(handler)
            {
                Timeout = requestTimeout                
            };
            return client;
        }

    }
}
