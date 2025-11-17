using System;
using System.Net.WebSockets;

namespace CryptoExchange.Net.Converters.MessageParsing.DynamicConverters
{
    /// <summary>
    /// Message converter
    /// </summary>
    public interface IMessageConverter
    {
        /// <summary>
        /// Get message info
        /// </summary>
        MessageInfo GetMessageInfo(ReadOnlySpan<byte> data, WebSocketMessageType? webSocketMessageType);

        /// <summary>
        /// Deserialize to the provided type
        /// </summary>
        object Deserialize(ReadOnlySpan<byte> data, Type type);
    }

}
