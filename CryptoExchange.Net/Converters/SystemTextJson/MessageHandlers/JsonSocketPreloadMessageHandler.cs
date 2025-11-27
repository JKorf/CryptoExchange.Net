using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using System;
using System.Net.WebSockets;
using System.Text.Json;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// JSON WebSocket message handler, reads the json data info a JsonDocument after which the data can be inspected to identify the message
    /// </summary>
    public abstract class JsonSocketPreloadMessageHandler : ISocketMessageHandler
    {
        /// <summary>
        /// The serializer options to use
        /// </summary>
        public abstract JsonSerializerOptions Options { get; }

        /// <inheritdoc />
        public virtual string? GetTypeIdentifier(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType)
        {
            var reader = new Utf8JsonReader(data);
            var jsonDocument = JsonDocument.ParseValue(ref reader);

            return GetTypeIdentifier(jsonDocument);
        }

        /// <summary>
        /// Get the message identifier for this document
        /// </summary>
        protected abstract string? GetTypeIdentifier(JsonDocument document);

        public virtual string? GetTopicFilter(object deserializedObject) => null;

        /// <inheritdoc />
        public virtual object Deserialize(ReadOnlySpan<byte> data, Type type)
        {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
            return JsonSerializer.Deserialize(data, type, Options)!;
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        }

        /// <summary>
        /// Get the string value for a path, or an emtpy string if not found
        /// </summary>
        protected string StringOrEmpty(JsonDocument document, string path)
        {
            if (!document.RootElement.TryGetProperty(path, out var element))
                return string.Empty;

            if (element.ValueKind == JsonValueKind.String)
                return element.GetString() ?? string.Empty;
            else if (element.ValueKind == JsonValueKind.Number)
                return element.GetDecimal().ToString();

            return string.Empty;
        }
    }
}
