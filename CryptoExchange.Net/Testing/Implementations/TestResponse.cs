using CryptoExchange.Net.Interfaces;
using System;
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
        public Version HttpVersion { get; }

        public bool IsSuccessStatusCode { get; }

        public long? ContentLength { get; }

        public KeyValuePair<string, string[]>[] ResponseHeaders { get; } = new KeyValuePair<string, string[]>[0];

        public TestResponse(HttpStatusCode code, Stream response)
        {
            StatusCode = code;
            HttpVersion = new Version(2, 0);
            IsSuccessStatusCode = code == HttpStatusCode.OK;
            _response = response;
        }

        public void Close()
        {
        }

        public Task<Stream> GetResponseStreamAsync() => Task.FromResult(_response);
    }
}
