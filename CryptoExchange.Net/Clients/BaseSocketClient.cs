using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base for socket client implementations
    /// </summary>
    public abstract class BaseSocketClient: BaseClient, ISocketClient
    {
        #region fields
        
        /// <summary>
        /// If client is disposing
        /// </summary>
        protected bool disposing;
        
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
        /// <param name="name">The name of the API this client is for</param>
        /// <param name="options">The options for this client</param>
        protected BaseSocketClient(string name, ClientOptions options) : base(name, options)
        {
        }

        /// <summary>
        /// Unsubscribe an update subscription
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to unsubscribe</param>
        /// <returns></returns>
        public virtual async Task UnsubscribeAsync(int subscriptionId)
        {
            foreach(var socket in ApiClients.OfType<SocketApiClient>())
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

            log.Write(LogLevel.Information, $"Socket {subscription.SocketId} Unsubscribing subscription  " + subscription.Id);
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
            log.Write(LogLevel.Information, $"Reconnecting all {CurrentConnections} connections");
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
            foreach(var client in ApiClients.OfType<SocketApiClient>())            
                result.AppendLine(client.GetSubscriptionsState());            
            return result.ToString();
        }
    }
}
