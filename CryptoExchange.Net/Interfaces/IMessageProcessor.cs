using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using System;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces;

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
    /// Handle a message
    /// </summary>
    Task<CallResult> Handle(SocketConnection connection, DataEvent<object> message, MessageHandlerLink matchedHandler);
    /// <summary>
    /// Deserialize a message into object of type
    /// </summary>
    /// <param name="accessor"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    CallResult<object> Deserialize(IMessageAccessor accessor, Type type);
}
