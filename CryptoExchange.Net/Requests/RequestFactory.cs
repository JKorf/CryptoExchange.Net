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
            {
                var handler = new HttpClientHandler();
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                if (proxy != null)
                {
                    handler.Proxy = new WebProxy
                    {
                        Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                        Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Login, proxy.Password)
                    };
                }

                client = new HttpClient(handler)
                {
                    Timeout = requestTimeout
                };
            }

            _httpClient = client;
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
