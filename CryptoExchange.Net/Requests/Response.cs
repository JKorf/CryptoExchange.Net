using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Requests
{
    /// <summary>
    /// HttpWebResponse response object
    /// </summary>
    public class Response : IResponse
    {
        private readonly HttpWebResponse response;

        /// <inheritdoc />
        public HttpStatusCode StatusCode => response.StatusCode;

        /// <summary>
        /// Create response for http web response
        /// </summary>
        /// <param name="response"></param>
        public Response(HttpWebResponse response)
        {
            this.response = response;
        }

        /// <inheritdoc />
        public Stream GetResponseStream()
        {
            return response.GetResponseStream();
        }

        /// <inheritdoc />
        public IEnumerable<Tuple<string, string>> GetResponseHeaders()
        {
            return response.Headers.ToIEnumerable();
        }

        /// <inheritdoc />
        public void Close()
        {
            response.Close();
        }
    }
}
