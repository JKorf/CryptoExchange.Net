using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Sockets;
using System;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Socket API client
    /// </summary>
    public interface ISocketApiClient: IBaseApiClient
    {
        /// <summary>
        /// The current amount of socket connections on the API client
        /// </summary>
        int CurrentConnections { get; }
        /// <summary>
        /// The current amount of subscriptions over all connections
        /// </summary>
        int CurrentSubscriptions { get; }
        /// <summary>
        /// Incoming data kpbs
        /// </summary>
        double IncomingKbps { get; }
        /// <summary>
        /// The factory for creating sockets. Used for unit testing
        /// </summary>
        IWebsocketFactory SocketFactory { get; set; }

        /// <summary>
        /// Get the url to reconnect to after losing a connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<Uri?> GetReconnectUriAsync(SocketConnection connection);

        /// <summary>
        /// Log the current state of connections and subscriptions
        /// </summary>
        string GetSubscriptionsState();
        /// <summary>
        /// Reconnect all connections
        /// </summary>
        /// <returns></returns>
        Task ReconnectAsync();
        /// <summary>
        /// Update the original request to send when the connection is restored after disconnecting. Can be used to update an authentication token for example.
        /// </summary>
        /// <param name="request">The original request</param>
        /// <returns></returns>
        Task<CallResult<object>> RevitalizeRequestAsync(object request);
        /// <summary>
        /// Periodically sends data over a socket connection
        /// </summary>
        /// <param name="identifier">Identifier for the periodic send</param>
        /// <param name="interval">How often</param>
        /// <param name="objGetter">Method returning the object to send</param>
        void SendPeriodic(string identifier, TimeSpan interval, Func<SocketConnection, object> objGetter);
        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        Task UnsubscribeAllAsync();
        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to unsubscribe</param>
        /// <returns></returns>
        Task<bool> UnsubscribeAsync(int subscriptionId);
        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe</param>
        /// <returns></returns>
        Task UnsubscribeAsync(UpdateSubscription subscription);
    }
}