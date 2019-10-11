using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Request interface
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Accept header
        /// </summary>
        string Accept { set; }
        /// <summary>
        /// Content
        /// </summary>
        string? Content { get; }
        /// <summary>
        /// Headers
        /// </summary>
        HttpRequestHeaders Headers { get; }
        /// <summary>
        /// Method
        /// </summary>
        HttpMethod Method { get; set; }
        /// <summary>
        /// Uri
        /// </summary>
        Uri Uri { get; }
        /// <summary>
        /// Set byte content
        /// </summary>
        /// <param name="data"></param>
        void SetContent(byte[] data);
        /// <summary>
        /// Set string content
        /// </summary>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        void SetContent(string data, string contentType);
        /// <summary>
        /// Get the response
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IResponse> GetResponse(CancellationToken cancellationToken);
    }
}
