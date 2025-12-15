using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        public Version HttpVersion => _response.Version;

        /// <inheritdoc />
        public bool IsSuccessStatusCode => _response.IsSuccessStatusCode;

        /// <inheritdoc />
        public long? ContentLength => _response.Content.Headers.ContentLength;

        /// <inheritdoc />
        public KeyValuePair<string, string[]>[] ResponseHeaders => _response.Headers.Select(x => new KeyValuePair<string, string[]>(x.Key, x.Value.ToArray())).ToArray();

        /// <summary>
        /// Create response for a http response message
        /// </summary>
        /// <param name="response">The actual response</param>
        public Response(HttpResponseMessage response)
        {
            this._response = response;
        }

        /// <inheritdoc />
        public async Task<Stream> GetResponseStreamAsync(CancellationToken cancellationToken)
        {
            #if NET5_0_OR_GREATER
                return await _response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            #else
                return await _response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            #endif
        }

        /// <inheritdoc />
        public void Close()
        {
            _response.Dispose();
        }
    }
}
