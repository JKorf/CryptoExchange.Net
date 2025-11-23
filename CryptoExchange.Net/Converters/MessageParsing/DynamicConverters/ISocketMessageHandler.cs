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
        /// Get an identifier for the message which can be used to link it to a listener
        /// </summary>
        string? GetMessageIdentifier(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);

        /// <summary>
        /// Deserialize to the provided type
        /// </summary>
        object Deserialize(ReadOnlySpan<byte> data, Type type);
    }

}
