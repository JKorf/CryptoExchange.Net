using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets.Default.Interfaces;
using CryptoExchange.Net.Sockets.HighPerf.Interfaces;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces.Clients
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
        /// Incoming data Kbps
        /// </summary>
        double IncomingKbps { get; }
        /// <summary>
        /// The factory for creating sockets. Used for unit testing
        /// </summary>
        IWebsocketFactory SocketFactory { get; set; }
        /// <summary>
        /// High performance websocket factory
        /// </summary>
        IHighPerfConnectionFactory? HighPerfConnectionFactory { get; set; }
        /// <summary>
        /// Current client options
        /// </summary>
        SocketExchangeOptions ClientOptions { get; }
        /// <summary>
        /// Current API options
        /// </summary>
        SocketApiOptions ApiOptions { get; }
        /// <summary>
        /// Log the current state of connections and subscriptions
        /// </summary>
        string GetSubscriptionsState(bool includeSubDetails = true);
        /// <summary>
        /// Reconnect all connections
        /// </summary>
        /// <returns></returns>
        Task ReconnectAsync();
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

        /// <summary>
        /// Prepare connections which can subsequently be used for sending websocket requests. Note that this is not required. If not prepared it will be initialized at the first websocket request.
        /// </summary>
        /// <returns></returns>
        Task<CallResult> PrepareConnectionsAsync();
    }
}