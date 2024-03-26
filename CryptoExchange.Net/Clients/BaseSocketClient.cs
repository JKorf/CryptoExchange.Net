using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Clients
{
    /// <summary>
    /// Base for socket client implementations
    /// </summary>
    public abstract class BaseSocketClient : BaseClient, ISocketClient
    {
        #region fields

        /// <summary>
        /// If client is disposing
        /// </summary>
        protected bool _disposing;

        /// <inheritdoc />
        public int CurrentConnections => ApiClients.OfType<SocketApiClient>().Sum(c => c.CurrentConnections);
        /// <inheritdoc />
        public int CurrentSubscriptions => ApiClients.OfType<SocketApiClient>().Sum(s => s.CurrentSubscriptions);
        /// <inheritdoc />
        public double IncomingKbps => ApiClients.OfType<SocketApiClient>().Sum(s => s.IncomingKbps);
        #endregion

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="exchange">The name of the exchange this client is for</param>
        protected BaseSocketClient(ILoggerFactory? logger, string exchange) : base(logger, exchange)
        {
        }

        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to unsubscribe</param>
        /// <returns></returns>
        public virtual async Task UnsubscribeAsync(int subscriptionId)
        {
            foreach (var socket in ApiClients.OfType<SocketApiClient>())
            {
                var result = await socket.UnsubscribeAsync(subscriptionId).ConfigureAwait(false);
                if (result)
                    break;
            }
        }

        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe</param>
        /// <returns></returns>
        public virtual async Task UnsubscribeAsync(UpdateSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            _logger.UnsubscribingSubscription(subscription.SocketId, subscription.Id);
            await subscription.CloseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe all subscriptions
        /// </summary>
        /// <returns></returns>
        public virtual async Task UnsubscribeAllAsync()
        {
            var tasks = new List<Task>();
            foreach (var client in ApiClients.OfType<SocketApiClient>())
                tasks.Add(client.UnsubscribeAllAsync());

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Reconnect all connections
        /// </summary>
        /// <returns></returns>
        public virtual async Task ReconnectAsync()
        {
            _logger.ReconnectingAllConnections(CurrentConnections);
            var tasks = new List<Task>();
            foreach (var client in ApiClients.OfType<SocketApiClient>())
            {
                tasks.Add(client.ReconnectAsync());
            }
            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Log the current state of connections and subscriptions
        /// </summary>
        public string GetSubscriptionsState()
        {
            var result = new StringBuilder();
            foreach (var client in ApiClients.OfType<SocketApiClient>().Where(c => c.CurrentSubscriptions > 0))
            {
                result.AppendLine(client.GetSubscriptionsState());
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns the state of all socket api clients
        /// </summary>
        /// <returns></returns>
        public List<SocketApiClient.SocketApiClientState> GetSocketApiClientStates()
        {
            var result = new List<SocketApiClient.SocketApiClientState>();
            foreach (var client in ApiClients.OfType<SocketApiClient>())
            {
                result.Add(client.GetState());
            }
            return result;
        }
    }
}
