using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// HTTP call result
    /// </summary>
    public interface IHttpResult : ICallResult
    {
        /// <summary>
        /// Exchange name
        /// </summary>
        string Exchange { get; init; }
        /// <summary>
        /// The original data returned by the call, only available when `OutputOriginalData` is set to `true` in the client options
        /// </summary>
        string? OriginalData { get; init; }
        /// <summary>
        /// The request http method
        /// </summary>
        HttpMethod? RequestMethod { get; init; }

        /// <summary>
        /// HTTP protocol version
        /// </summary>
        Version? HttpVersion { get; init; }

        /// <summary>
        /// The headers sent with the request
        /// </summary>
        HttpRequestHeaders? RequestHeaders { get; init; }

        /// <summary>
        /// The request id
        /// </summary>
        int? RequestId { get; init; }

        /// <summary>
        /// The url which was requested
        /// </summary>
        string? RequestUrl { get; init; }

        /// <summary>
        /// The body of the request
        /// </summary>
        string? RequestBody { get; init; }

        /// <summary>
        /// Length in bytes of the response
        /// </summary>
        long? ResponseLength { get; init; }

        /// <summary>
        /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
        /// </summary>
        HttpStatusCode? ResponseStatusCode { get; init; }

        /// <summary>
        /// The response headers
        /// </summary>
        HttpResponseHeaders? ResponseHeaders { get; init; }

        /// <summary>
        /// The time between sending the request and receiving the response
        /// </summary>
        TimeSpan? ResponseTime { get; init; }
        /// <summary>
        /// The data source of this result
        /// </summary>
        ResultDataSource DataSource { get; init; }
    }

    /// <summary>
    /// HTTP call result
    /// </summary>
    /// <typeparam name="T">Result data type</typeparam>
    public interface IHttpResult<T> : IHttpResult, ICallResult<T>
    {
    }
}
