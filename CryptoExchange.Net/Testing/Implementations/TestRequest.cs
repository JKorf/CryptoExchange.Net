using CryptoExchange.Net.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestRequest : IRequest
    {
        private readonly HttpRequestHeaders _headers = new HttpRequestMessage().Headers;
        private readonly TestResponse _response;

        public MediaTypeWithQualityHeaderValue Accept { set { } }

        public string? Content { get; private set; }

        public HttpMethod Method { get; set; }

        public Uri Uri { get; set; }

        public Version HttpVersion { get; set; }

        public int RequestId { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TestRequest(TestResponse response)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _response = response;
        }

        public void AddHeader(string key, string value)
        {
            _headers.Add(key, value);
        }

        public HttpRequestHeaders GetHeaders() => _headers;

        public Task<IResponse> GetResponseAsync(CancellationToken cancellationToken) => Task.FromResult<IResponse>(_response);

        public void SetContent(byte[] data)
        {
            Content = Encoding.UTF8.GetString(data);
        }

        public void SetContent(string data, string contentType)
        {
            Content = data;
        }
    }
}
