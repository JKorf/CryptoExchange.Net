using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    /// <summary>
    /// REST message handler
    /// </summary>
    public interface IRestMessageHandler
    {
        /// <summary>
        /// The `accept` HTTP response header for the request
        /// </summary>
        MediaTypeWithQualityHeaderValue AcceptHeader { get; }

        /// <summary>
        /// Create an object to keep state for a request
        /// </summary>
        /// <returns></returns>
        object? CreateState();

        /// <summary>
        /// Parse the response when the HTTP response status indicated an error
        /// </summary>
        ValueTask<Error> ParseErrorResponse(
            int httpStatusCode,
            object? state,
            HttpResponseHeaders responseHeaders,
            Stream responseStream);

        /// <summary>
        /// Parse the response when the HTTP response status indicated a rate limit error
        /// </summary>
        ValueTask<ServerRateLimitError> ParseErrorRateLimitResponse(
            int httpStatusCode,
            object? state,
            HttpResponseHeaders responseHeaders,
            Stream responseStream);

        /// <summary>
        /// Check if the response is an error response; if so return the error
        /// </summary>
        ValueTask<Error?> CheckForErrorResponse(
            RequestDefinition request,
            object? state,
            HttpResponseHeaders responseHeaders,
            Stream responseStream);

        /// <summary>
        /// Deserialize the response stream
        /// </summary>
        ValueTask<(T? Result, Error? Error)> TryDeserializeAsync<T>(
            Stream responseStream,
            object? state,
            CancellationToken ct);
    }

}
