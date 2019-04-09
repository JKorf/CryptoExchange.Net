using System;
using System.Threading.Tasks;
using CryptoExchange.Net.Sockets;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Base class for socket API implementations
    /// </summary>
    public interface ISocketClient: IDisposable
    {
        /// <summary>
        /// The factory for creating sockets. Used for unit testing
        /// </summary>
        IWebsocketFactory SocketFactory { get; set; }

        /// <summary>
        /// The time in between reconnect attempts
        /// </summary>
        TimeSpan ReconnectInterval { get; }
        
        /// <summary>
        /// Whether the client should try to auto reconnect when losing connection
        /// </summary>
        bool AutoReconnect { get; }

        /// <summary>
        /// The base address of the API
        /// </summary>
        string BaseAddress { get; }

        /// <summary>
        /// Unsubscribe from a stream
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe</param>
        /// <returns></returns>
        Task Unsubscribe(UpdateSubscription subscription);

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        Task UnsubscribeAll();
    }
}