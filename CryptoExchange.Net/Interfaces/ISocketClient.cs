using System;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Base class for socket API implementations
    /// </summary>
    public interface ISocketClient: IDisposable
    {
        /// <summary>
        /// The exchange name
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// The options provided for this client
        /// </summary>
        ExchangeOptions ClientOptions { get; }

        /// <summary>
        /// Incoming kilobytes per second of data
        /// </summary>
        public double IncomingKbps { get; }

        /// <summary>
        /// The current amount of connections to the API from this client. A connection can have multiple subscriptions.
        /// </summary>
        public int CurrentConnections { get; }
        
        /// <summary>
        /// The current amount of subscriptions running from the client
        /// </summary>
        public int CurrentSubscriptions { get; }

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