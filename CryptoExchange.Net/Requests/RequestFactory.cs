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
        public void Configure(ApiProxy? proxy, TimeSpan requestTimeout, HttpClient? client = null)
        {
            if (client == null)
                client = CreateClient(proxy, requestTimeout);

            _httpClient = client;
        }

        /// <inheritdoc />
        public IRequest Create(HttpMethod method, Uri uri, int requestId)
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Cant create request before configuring http client");

            return new Request(new HttpRequestMessage(method, uri), _httpClient, requestId);
        }

        /// <inheritdoc />
        public void UpdateSettings(ApiProxy? proxy, TimeSpan requestTimeout)
        {
            _httpClient = CreateClient(proxy, requestTimeout);
        }

        private static HttpClient CreateClient(ApiProxy? proxy, TimeSpan requestTimeout)
        {
            var handler = new HttpClientHandler();
            try
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            }
            catch (PlatformNotSupportedException) { }
            catch (NotImplementedException) { } // Mono runtime throws NotImplementedException

            if (proxy != null)
            {
                handler.Proxy = new WebProxy
                {
                    Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                    Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Login, proxy.Password)
                };
            }

            var client = new HttpClient(handler)
            {
                Timeout = requestTimeout
            };
            return client;
        }
    }
}
