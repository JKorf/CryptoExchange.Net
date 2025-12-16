using System;
using System.Net.WebSockets;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    /// <summary>
    /// WebSocket message handler
    /// </summary>
    public interface ISocketMessageHandler
    {
        /// <summary>
        /// Get an identifier for the message which can be used to determine the type of the message
        /// </summary>
        string? GetTypeIdentifier(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);

        /// <summary>
        /// Get optional topic filter, for example a symbol name
        /// </summary>
        string? GetTopicFilter(object deserializedObject);

        /// <summary>
        /// Deserialize to the provided type
        /// </summary>
        object Deserialize(ReadOnlySpan<byte> data, Type type);
    }

}
