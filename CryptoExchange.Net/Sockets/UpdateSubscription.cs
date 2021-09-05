using CryptoExchange.Net.Objects;
using System;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Subscription to a data stream
    /// </summary>
    public class UpdateSubscription
    {
        private readonly SocketConnection connection;
        private readonly SocketSubscription subscription;

        /// <summary>
        /// Event when the connection is lost. The socket will automatically reconnect when possible.
        /// </summary>
        public event Action ConnectionLost
        {
            add => connection.ConnectionLost += value;
            remove => connection.ConnectionLost -= value;
        }

        /// <summary>
        /// Event when the connection is closed. This event happens when reconnecting/resubscribing has failed too often based on the <see cref="SocketClientOptions.MaxReconnectTries"/> and <see cref="SocketClientOptions.MaxResubscribeTries"/> options,
        /// or <see cref="SocketClientOptions.AutoReconnect"/> is false
        /// </summary>
        public event Action ConnectionClosed
        {
            add => connection.ConnectionClosed += value;
            remove => connection.ConnectionClosed -= value;
        }

        /// <summary>
        /// Event when the connection is restored. Timespan parameter indicates the time the socket has been offline for before reconnecting. 
        /// Note that when the executing code is suspended and resumed at a later period (for example laptop going to sleep) the disconnect time will be incorrect as the diconnect
        /// will only be detected after resuming. This will lead to an incorrect disconnected timespan.
        /// </summary>
        public event Action<TimeSpan> ConnectionRestored
        {
            add => connection.ConnectionRestored += value;
            remove => connection.ConnectionRestored -= value;
        }

        /// <summary>
        /// Event when the connection to the server is paused based on a server indication. No operations can be performed while paused
        /// </summary>
        public event Action ActivityPaused
        {
            add => connection.ActivityPaused += value;
            remove => connection.ActivityPaused -= value;
        }

        /// <summary>
        /// Event when the connection to the server is unpaused after being paused
        /// </summary>
        public event Action ActivityUnpaused
        {
            add => connection.ActivityUnpaused += value;
            remove => connection.ActivityUnpaused -= value;
        }

        /// <summary>
        /// Event when an exception happens during the handling of the data
        /// </summary>
        public event Action<Exception> Exception
        {
            add => subscription.Exception += value;
            remove => subscription.Exception -= value;
        }

        /// <summary>
        /// The id of the socket
        /// </summary>
        public int SocketId => connection.Socket.Id;

        /// <summary>
        /// The id of the subscription
        /// </summary>
        public int Id => subscription.Id;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connection">The socket connection the subscription is on</param>
        /// <param name="subscription">The subscription</param>
        public UpdateSubscription(SocketConnection connection, SocketSubscription subscription)
        {
            this.connection = connection;
            this.subscription = subscription;
        }
        
        /// <summary>
        /// Close the subscription
        /// </summary>
        /// <returns></returns>
        public Task CloseAsync()
        {
            return connection.CloseAsync(subscription);
        }

        /// <summary>
        /// Close the socket to cause a reconnect
        /// </summary>
        /// <returns></returns>
        internal Task ReconnectAsync()
        {
            return connection.Socket.CloseAsync();
        }

        /// <summary>
        /// Unsubscribe a subscription
        /// </summary>
        /// <returns></returns>
        internal async Task UnsubscribeAsync()
        {
            await connection.UnsubscribeAsync(subscription).ConfigureAwait(false);
        }

        /// <summary>
        /// Resubscribe this subscription
        /// </summary>
        /// <returns></returns>
        internal async Task<CallResult<bool>> ResubscribeAsync()
        {
            return await connection.ResubscribeAsync(subscription).ConfigureAwait(false);
        }
    }
}
