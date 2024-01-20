using CryptoExchange.Net.Sockets.MessageParsing.Interfaces;
using System;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Message received from the websocket
    /// </summary>
    public class SocketMessage
    {
        /// <summary>
        /// Message receive time
        /// </summary>
        public DateTime ReceiveTime { get; set; }
        /// <summary>
        /// The message data
        /// </summary>
        public IMessageAccessor Message { get; set; }
        /// <summary>
        /// Raw string data
        /// </summary>
        public string? RawData { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="receiveTime"></param>
        /// <param name="message"></param>
        public SocketMessage(DateTime receiveTime, IMessageAccessor message)
        {
            ReceiveTime = receiveTime;
            Message = message;
        }

        /// <summary>
        /// Deserialize the message to a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Deserialize(Type type)
        {
            return Message.Deserialize(type);
        }
    }
}
