using System;
using System.Collections.Generic;
using System.Net.Http;
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
        /// Method
        /// </summary>
        HttpMethod Method { get; set; }
        /// <summary>
        /// Uri
        /// </summary>
        Uri Uri { get; }
        /// <summary>
        /// internal request id for tracing
        /// </summary>
        int RequestId { get; }
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
        /// Add a header to the request
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void AddHeader(string key, string value);

        /// <summary>
        /// Get all headers
        /// </summary>
        /// <returns></returns>
        Dictionary<string, IEnumerable<string>> GetHeaders();

        /// <summary>
        /// Get the response
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IResponse> GetResponseAsync(CancellationToken cancellationToken);
    }
}
