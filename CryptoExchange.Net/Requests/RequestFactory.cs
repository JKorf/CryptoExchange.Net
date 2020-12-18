﻿using System;
using System.Net;
using System.Net.Http;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// WebRequest factory
    /// </summary>
    public class RequestFactory : IRequestFactory
    {
        private HttpClient? httpClient;
        private IHttpClientFactory? httpClientFactory;
        private string clientName;       
        /// <inheritdoc />
        public void Configure(string clientName, TimeSpan requestTimeout, ApiProxy? proxy)
        {
            this.clientName = clientName;
            HttpMessageHandler handler = new HttpClientHandler()
            {
                Proxy = proxy == null ? null : new WebProxy
                {
                    Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                    Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Login, proxy.Password)
                }
            };
            httpClient = new HttpClient(handler) { Timeout = requestTimeout };

        }
        /// <inheritdoc />
        public void Configure(string clientName, IHttpClientFactory factory)
        {
            this.clientName = clientName;
            httpClientFactory = factory;
        }
        /// <inheritdoc />
        public IRequest Create(HttpMethod method, string uri, int requestId)
        {
            if (httpClient == null)
                throw new InvalidOperationException("Cant create request before configuring http client");

            if (httpClientFactory != null)
                httpClient = httpClientFactory.CreateClient(clientName);

            return new Request(new HttpRequestMessage(method, uri), httpClient, requestId);
        }
    }
}
