using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets.Default;
using System;

namespace CryptoExchange.Net.Sockets.Interfaces
{
    /// <summary>
    /// Message processor
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Id of the processor
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// The message router for this processor
        /// </summary>
        public MessageRouter MessageRouter { get; }
        /// <summary>
        /// Handle a message
        /// </summary>
        CallResult? Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object result, MessageRoute route);
    }
}
