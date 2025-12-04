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
        /// Whether a seekable stream is required
        /// </summary>
        bool RequiresSeekableStream { get; }

        /// <summary>
        /// Parse the response when the HTTP response status indicated an error
        /// </summary>
        ValueTask<Error> ParseErrorResponse(
            int httpStatusCode,
            HttpResponseHeaders responseHeaders,
            Stream responseStream);

        /// <summary>
        /// Parse the response when the HTTP response status indicated a rate limit error
        /// </summary>
        ValueTask<ServerRateLimitError> ParseErrorRateLimitResponse(
            int httpStatusCode,
            HttpResponseHeaders responseHeaders,
            Stream responseStream);

        /// <summary>
        /// Check if the response is an error response; if so return the error.<br />
        /// Note that if the API returns a standard result wrapper, something like this:
        /// <code>{ "code": 400, "msg": "error", "data": {} }</code> 
        /// then the `CheckDeserializedResponse` method should be used for checking the result
        /// </summary>
        ValueTask<Error?> CheckForErrorResponse(
            RequestDefinition request,
            HttpResponseHeaders responseHeaders,
            Stream responseStream);

        /// <summary>
        /// Deserialize the response stream
        /// </summary>
        ValueTask<(T? Result, Error? Error)> TryDeserializeAsync<T>(
            Stream responseStream,
            CancellationToken ct);

        /// <summary>
        /// Check whether the resulting T object indicates an error or not
        /// </summary>
        Error? CheckDeserializedResponse<T>(HttpResponseHeaders responseHeaders, T result);
    }

}
