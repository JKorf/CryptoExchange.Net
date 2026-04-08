using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
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
        /// Event when the message router for this processor has been changed
        /// </summary>
        public event Action? OnMessageRouterUpdated;
        /// <summary>
        /// Handle a message
        /// </summary>
        bool Handle(string typeIdentifier, string? topicFilter, SocketConnection socketConnection, DateTime receiveTime, string? originalData, object result);
    }
}
