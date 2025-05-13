using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestRequest : IRequest
    {
        private readonly List<KeyValuePair<string, string[]>> _headers = new();
        private readonly TestResponse _response;

        public string Accept { set { } }

        public string? Content { get; private set; }

        public HttpMethod Method { get; set; }

        public Uri Uri { get; set; }

        public int RequestId { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TestRequest(TestResponse response)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _response = response;
        }

        public void AddHeader(string key, string value)
        {
            _headers.Add(new KeyValuePair<string, string[]>(key, new[] { value }));
        }

        public KeyValuePair<string, string[]>[] GetHeaders() => _headers.ToArray();

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
