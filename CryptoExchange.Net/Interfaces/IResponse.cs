using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Response object interface
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// The response status code
        /// </summary>
        HttpStatusCode StatusCode { get; }
        /// <summary>
        /// Get the response stream
        /// </summary>
        /// <returns></returns>
        Stream GetResponseStream();
        /// <summary>
        /// Get the response headers
        /// </summary>
        /// <returns></returns>
        IEnumerable<Tuple<string, string>> GetResponseHeaders();
        /// <summary>
        /// Close the response
        /// </summary>
        void Close();
    }
}
