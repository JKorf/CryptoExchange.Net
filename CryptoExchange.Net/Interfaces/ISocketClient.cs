using System;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
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

        /// <inheritdoc cref="SocketClientOptions.SocketResponseTimeout"/>
        TimeSpan ResponseTimeout { get; }

        /// <inheritdoc cref="SocketClientOptions.SocketNoDataTimeout"/>
        TimeSpan SocketNoDataTimeout { get; }

        /// <summary>
        /// The max amount of concurrent socket connections
        /// </summary>
        int MaxSocketConnections { get; }

        /// <inheritdoc cref="SocketClientOptions.SocketSubscriptionsCombineTarget"/>
        int SocketCombineTarget { get; }
        /// <inheritdoc cref="SocketClientOptions.MaxReconnectTries"/>
        int? MaxReconnectTries { get; }
        /// <inheritdoc cref="SocketClientOptions.MaxResubscribeTries"/>
        int? MaxResubscribeTries { get; }
        /// <inheritdoc cref="SocketClientOptions.MaxConcurrentResubscriptionsPerSocket"/>
        int MaxConcurrentResubscriptionsPerSocket { get; }
        /// <summary>
        /// The current kilobytes per second of data being received by all connection from this client, averaged over the last 3 seconds
        /// </summary>
        double IncomingKbps { get; }

        /// <summary>
        /// Unsubscribe from a stream
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe</param>
        /// <returns></returns>
        Task UnsubscribeAsync(UpdateSubscription subscription);

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        Task UnsubscribeAllAsync();
    }
}