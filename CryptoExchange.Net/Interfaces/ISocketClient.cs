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
        /// The options provided for this client
        /// </summary>
        BaseSocketClientOptions ClientOptions { get; }

        /// <summary>
        /// Incoming kilobytes per second of data
        /// </summary>
        public double IncomingKbps { get; }

        /// <summary>
        /// Unsubscribe from a stream using the subscription id received when starting the subscription
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to unsubscribe</param>
        /// <returns></returns>
        Task UnsubscribeAsync(int subscriptionId);

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