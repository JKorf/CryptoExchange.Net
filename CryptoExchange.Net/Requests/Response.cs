using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// HttpWebResponse response object
    /// </summary>
    internal class Response : IResponse
    {
        private readonly HttpResponseMessage response;

        /// <inheritdoc />
        public HttpStatusCode StatusCode => response.StatusCode;

        /// <inheritdoc />
        public bool IsSuccessStatusCode => response.IsSuccessStatusCode;

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ResponseHeaders => response.Headers;

        /// <summary>
        /// Create response for a http response message
        /// </summary>
        /// <param name="response">The actual response</param>
        public Response(HttpResponseMessage response)
        {
            this.response = response;
        }

        /// <inheritdoc />
        public async Task<Stream> GetResponseStreamAsync()
        {
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Close()
        {
            response.Dispose();
        }
    }
}
