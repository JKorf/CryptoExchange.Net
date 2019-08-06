using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Request interface
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// The uri of the request
        /// </summary>
        Uri Uri { get; }
        /// <summary>
        /// The headers of the request
        /// </summary>
        WebHeaderCollection Headers { get; set; }
        /// <summary>
        /// The method of the request
        /// </summary>
        string Method { get; set; }
        /// <summary>
        /// The timeout of the request
        /// </summary>
        TimeSpan Timeout { get; set; }
        /// <summary>
        /// Set a proxy
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="login"></param>
        /// <param name="password"></param>
        void SetProxy(string host, int port, string login, string password);

        /// <summary>
        /// Content type
        /// </summary>
        string ContentType { get; set; }
        /// <summary>
        /// String content
        /// </summary>
        string Content { get; set; }
        /// <summary>
        /// Accept
        /// </summary>
        string Accept { get; set; }
        /// <summary>
        /// Content length
        /// </summary>
        long ContentLength { get; set; }

        /// <summary>
        /// Get the request stream
        /// </summary>
        /// <returns></returns>
        Task<Stream> GetRequestStream();
        /// <summary>
        /// Get the response object
        /// </summary>
        /// <returns></returns>
        Task<IResponse> GetResponse();
    }
}
