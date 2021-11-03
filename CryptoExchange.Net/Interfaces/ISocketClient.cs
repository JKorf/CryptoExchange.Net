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
        /// Client options
        /// </summary>
        SocketClientOptions ClientOptions { get; }

        /// <summary>
        /// Incoming kilobytes per second of data
        /// </summary>
        public double IncomingKbps { get; }

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