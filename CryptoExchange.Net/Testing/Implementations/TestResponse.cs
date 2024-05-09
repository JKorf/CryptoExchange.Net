using CryptoExchange.Net.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestResponse : IResponse
    {
        private readonly Stream _response;

        public HttpStatusCode StatusCode { get; }

        public bool IsSuccessStatusCode { get; }

        public long? ContentLength { get; }

        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ResponseHeaders { get; } = new Dictionary<string, IEnumerable<string>>();

        public TestResponse(HttpStatusCode code, Stream response)
        {
            StatusCode = code;
            IsSuccessStatusCode = code == HttpStatusCode.OK;
            _response = response;
        }

        public void Close()
        {
        }

        public Task<Stream> GetResponseStreamAsync() => Task.FromResult(_response);
    }
}
