using System;

namespace CryptoExchange.Net.Sockets.MessageParsing.Interfaces
{
    /// <summary>
    /// Message accessor
    /// </summary>
    public interface IMessageAccessor
    {
        /// <summary>
        /// Is this a json message
        /// </summary>
        bool IsJson { get; }
        /// <summary>
        /// Get the type of node
        /// </summary>
        /// <returns></returns>
        NodeType? GetNodeType();
        /// <summary>
        /// Get the type of node
        /// </summary>
        /// <param name="path">Access path</param>
        /// <returns></returns>
        NodeType? GetNodeType(MessagePath path);
        /// <summary>
        /// Get the value of a path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        T? GetValue<T>(MessagePath path);
        /// <summary>
        /// Deserialize the message into this type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        object Deserialize(Type type);
    }
}
