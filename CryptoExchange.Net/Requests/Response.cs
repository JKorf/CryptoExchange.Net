using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// Response object, wrapper for HttpResponseMessage
    /// </summary>
    internal class Response : IResponse
    {
        private readonly HttpResponseMessage _response;

        /// <inheritdoc />
        public HttpStatusCode StatusCode => _response.StatusCode;

        /// <inheritdoc />
        public bool IsSuccessStatusCode => _response.IsSuccessStatusCode;

        /// <inheritdoc />
        public long? ContentLength => _response.Content.Headers.ContentLength;

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ResponseHeaders => _response.Headers;

        /// <summary>
        /// Create response for a http response message
        /// </summary>
        /// <param name="response">The actual response</param>
        public Response(HttpResponseMessage response)
        {
            this._response = response;
        }

        /// <inheritdoc />
        public async Task<Stream> GetResponseStreamAsync()
        {
            return await _response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Close()
        {
            _response.Dispose();
        }
    }
}
