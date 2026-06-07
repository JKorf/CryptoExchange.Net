using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// WebSocket call result
    /// </summary>
    public interface IWebSocketResult : ICallResult
    {
        /// <summary>
        /// Exchange name
        /// </summary>
        string Exchange { get; init; }

        /// <summary>
        /// The request id
        /// </summary>
        public int? RequestId { get; init; }

        /// <summary>
        /// The url which was requested
        /// </summary>
        public int? ConnectionId { get; init; }

        /// <summary>
        /// The websocket url
        /// </summary>
        public string? Url { get; init; }

        /// <summary>
        /// The time between sending the request and receiving the response
        /// </summary>
        public TimeSpan? ResponseTime { get; init; }
    }

    /// <summary>
    /// WebSocket call result
    /// </summary>
    /// <typeparam name="T">Data result type</typeparam>
    public interface IWebSocketResult<T> : IWebSocketResult, ICallResult<T>
    {

    }

    /// <summary>
    /// Query result
    /// </summary>
    public interface IQueryResult : IWebSocketResult
    {
        /// <summary>
        /// The original returned data, only available when OutputOriginalData is set to true in the client options
        /// </summary>
        public string? OriginalData { get; init; }
        /// <summary>
        /// The query request body
        /// </summary>
        public string? RequestBody { get; init; }
    }

    /// <summary>
    /// Query result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueryResult<T> : IQueryResult, IWebSocketResult<T>
    {
    }
}
