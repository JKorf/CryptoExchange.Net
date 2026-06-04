using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CryptoExchange.Net.Objects
{
    public interface IWebSocketResult : ICallResult
    {
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
}
