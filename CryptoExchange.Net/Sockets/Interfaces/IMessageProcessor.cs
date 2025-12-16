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
        /// The matcher for this listener
        /// </summary>
        public MessageMatcher MessageMatcher { get; }
        /// <summary>
        /// The message router for this processor
        /// </summary>
        public MessageRouter MessageRouter { get; }
        /// <summary>
        /// Handle a message
        /// </summary>
        CallResult Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object result, MessageHandlerLink matchedHandler);
        /// <summary>
        /// Handle a message
        /// </summary>
        CallResult? Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object result, MessageRoute route);
        /// <summary>
        /// Deserialize a message into object of type
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        CallResult<object> Deserialize(IMessageAccessor accessor, Type type);
    }
}
