using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Converters.SystemTextJson.MessageConverters
{
    internal class JsonDocState
    {
        public JsonDocument? Document { get; set; }
    }

    /// <summary>
    /// JSON REST message handler
    /// </summary>
    public abstract class JsonRestMessageHandler : IRestMessageHandler
    {
        private static MediaTypeWithQualityHeaderValue _acceptJsonContent = new MediaTypeWithQualityHeaderValue(Constants.JsonContentHeader);

        /// <summary>
        /// Empty rate limit error
        /// </summary>
        protected static readonly ServerRateLimitError _emptyRateLimitError = new ServerRateLimitError();

        /// <summary>
        /// The serializer options to use
        /// </summary>
        public abstract JsonSerializerOptions Options { get; }

        /// <inheritdoc />
        public MediaTypeWithQualityHeaderValue AcceptHeader => _acceptJsonContent;

        /// <inheritdoc />
        public virtual object CreateState() => new JsonDocState();

        /// <inheritdoc />
        public virtual ValueTask<ServerRateLimitError> ParseErrorRateLimitResponse(
            int httpStatusCode,
            object? state,
            HttpResponseHeaders responseHeaders,
            Stream responseStream)
        {
            // Handle retry after header
            var retryAfterHeader = responseHeaders.SingleOrDefault(r => r.Key.Equals("Retry-After", StringComparison.InvariantCultureIgnoreCase));
            if (retryAfterHeader.Value?.Any() != true)
                return new ValueTask<ServerRateLimitError>(_emptyRateLimitError);

            var value = retryAfterHeader.Value.First();
            if (int.TryParse(value, out var seconds))
                return new ValueTask<ServerRateLimitError>(new ServerRateLimitError() { RetryAfter = DateTime.UtcNow.AddSeconds(seconds) });

            if (DateTime.TryParse(value, out var datetime))
                return new ValueTask<ServerRateLimitError>(new ServerRateLimitError() { RetryAfter = datetime });

            return new ValueTask<ServerRateLimitError>(_emptyRateLimitError);
        }

        /// <inheritdoc />
        public abstract ValueTask<Error> ParseErrorResponse(
            int httpStatusCode,
            object? state,
            HttpResponseHeaders responseHeaders,
            Stream responseStream);

        /// <inheritdoc />
        public virtual ValueTask<Error?> CheckForErrorResponse(
            RequestDefinition request,
            object? state,
            HttpResponseHeaders responseHeaders,
            Stream responseStream) => new ValueTask<Error?>((Error?)null);

        /// <summary>
        /// Read the response into a JsonDocument object
        /// </summary>
        protected virtual async ValueTask<(Error?, JsonDocument?)> GetJsonDocument(Stream stream, object? state)
        {
            if (state is JsonDocState documentState && documentState.Document != null)
                return (null, documentState.Document);

            try
            {
                var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
                ((JsonDocState)state!).Document = document;
                return (null, document);
            }
            catch (Exception ex)
            {
                return (new ServerError(new ErrorInfo(ErrorType.DeserializationFailed, false, "Deserialization failed, invalid JSON"), ex), null);
            }
        }

        /// <inheritdoc />
        public async ValueTask<(T? Result, Error? Error)> TryDeserializeAsync<T>(Stream responseStream, object? state, CancellationToken cancellationToken)
        {
            try
            {
                // If the document was already loaded (because we needed it for checking a response code for instance)
                // then we deserialize from the document, else from the stream
                T result;
                if (state is JsonDocState documentState && documentState.Document != null)
                {
                    result = documentState.Document.Deserialize<T>(Options)!;
                }
                else
                {
                    result = await JsonSerializer.DeserializeAsync<T>(responseStream, Options)!.ConfigureAwait(false)!;
                }
                return (result, null);
            }
            catch (HttpRequestException requestException)
            {
                // Request exception, can't reach server for instance
                var error = new WebError(requestException.Message, requestException);
                return (default, error);
            }
            catch (OperationCanceledException canceledException)
            {
                if (cancellationToken != default && canceledException.CancellationToken == cancellationToken)
                {
                    // Cancellation token canceled by caller
                    return (default, new CancellationRequestedError(canceledException));
                }
                else
                {
                    // Request timed out
                    var error = new WebError($"Request timed out", exception: canceledException);
                    error.ErrorType = ErrorType.Timeout;
                    return (default, error);
                }
            }
            catch (ArgumentException argumentException)
            {
                if (argumentException.Message.StartsWith("Only HTTP/"))
                {
                    // Unsupported HTTP version error .net framework
                    var error = ArgumentError.Invalid(nameof(RestExchangeOptions.HttpVersion), $"Invalid HTTP version: " + argumentException.Message);
                    return (default, error);
                }

                throw;
            }
            catch (NotSupportedException notSupportedException)
            {
                if (notSupportedException.Message.StartsWith("Request version value must be one of"))
                {
                    // Unsupported HTTP version error dotnet code
                    var error = ArgumentError.Invalid(nameof(RestExchangeOptions.HttpVersion), $"Invalid HTTP version: " + notSupportedException.Message);
                    return (default, error);
                }

                throw;
            }
        }
    }
}
