using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Converters.SystemTextJson.MessageHandlers
{
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

        /// <inheritdoc />
        public virtual bool RequiresSeekableStream => false;

        /// <summary>
        /// The serializer options to use
        /// </summary>
        public abstract JsonSerializerOptions Options { get; }

        /// <inheritdoc />
        public MediaTypeWithQualityHeaderValue AcceptHeader => _acceptJsonContent;

        /// <inheritdoc />
        public virtual ValueTask<ServerRateLimitError> ParseErrorRateLimitResponse(
            int httpStatusCode,
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
            HttpResponseHeaders responseHeaders,
            Stream responseStream);

        /// <inheritdoc />
        public virtual ValueTask<Error?> CheckForErrorResponse(
            RequestDefinition request,
            HttpResponseHeaders responseHeaders,
            Stream responseStream) => new ValueTask<Error?>((Error?)null);

        /// <summary>
        /// Read the response into a JsonDocument object
        /// </summary>
        protected virtual async ValueTask<(Error?, JsonDocument?)> GetJsonDocument(Stream stream)
        {
            try
            {
                var document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
                return (null, document);
            }
            catch (Exception ex)
            {
                return (new ServerError(new ErrorInfo(ErrorType.DeserializationFailed, false, "Deserialization failed, invalid JSON"), ex), null);
            }
        }

        /// <inheritdoc />
#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL3050:RequiresUnreferencedCode", Justification = "JsonSerializerOptions provided here has TypeInfoResolver set")]
        [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "JsonSerializerOptions provided here has TypeInfoResolver set")]
#endif
        public async ValueTask<(T? Result, Error? Error)> TryDeserializeAsync<T>(Stream responseStream, CancellationToken cancellationToken)
        {
            try
            {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
                var result = await JsonSerializer.DeserializeAsync<T>(responseStream, Options)!.ConfigureAwait(false)!;
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                return (result, null);
            }            
            catch (JsonException ex)
            {
                var info = $"Json deserialization failed: {ex.Message}, Path: {ex.Path}, LineNumber: {ex.LineNumber}, LinePosition: {ex.BytePositionInLine}";
                return (default, new DeserializeError(info, ex));
            }
            catch (Exception ex)
            {
                return (default, new DeserializeError($"Json deserialization failed: {ex.Message}", ex));
            }
        }

        /// <inheritdoc />
        public virtual Error? CheckDeserializedResponse<T>(HttpResponseHeaders responseHeaders, T result) => null;
    }
}
