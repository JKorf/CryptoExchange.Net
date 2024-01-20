using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
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
        /// The identifiers for this processor
        /// </summary>
        public HashSet<string> ListenerIdentifiers { get; }
        /// <summary>
        /// Handle a message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<CallResult> HandleAsync(SocketConnection connection, DataEvent<object> message);
        /// <summary>
        /// Get the type the message should be deserialized to
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Type? GetMessageType(SocketMessage message);
    }
}
