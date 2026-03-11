using System;
using System.Net.Http;
using CryptoExchange.Net.Authentication;
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
        private RestExchangeOptions? _options;

        /// <inheritdoc />
        public void Configure(RestExchangeOptions options, HttpClient? client = null)
        {
            if (client == null)
                client = CreateClient(options);

            _httpClient = client;
            _options = options;
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
            var newOptions = new RestExchangeOptions();
            _options!.Set(newOptions);
            newOptions.Proxy = proxy;
            newOptions.RequestTimeout = requestTimeout;
            newOptions.HttpKeepAliveInterval = httpKeepAliveInterval;
            _httpClient = CreateClient(newOptions);
        }

        private static HttpClient CreateClient(RestExchangeOptions options)
        {
            var handler = LibraryHelpers.CreateHttpClientMessageHandler(options);
            var client = new HttpClient(handler)
            {
                Timeout = options.RequestTimeout
            };
            return client;
        }

    }
}
