using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using System;
using System.Net.Http;

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestRequestFactory : IRequestFactory
    {
        private readonly TestRequest _request;

        public TestRequestFactory(TestRequest request)
        {
            _request = request;
        }

        public void Configure(RestExchangeOptions options, HttpClient? client)
        { 
        }

        public IRequest Create(Version httpRequestVersion, HttpMethod method, Uri uri, int requestId)
        {
            _request.Method = method;
            _request.Uri = uri;
            _request.RequestId = requestId;
            return _request;
        }

        public void UpdateSettings(ApiProxy? proxy, TimeSpan requestTimeout, TimeSpan? httpKeepAliveInterval) {}
    }
}
