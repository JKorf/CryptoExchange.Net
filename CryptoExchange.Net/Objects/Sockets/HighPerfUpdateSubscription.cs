using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// Subscription to a data stream
    /// </summary>
    public class HighPerfUpdateSubscription
    {
        private readonly HighPerfSocketConnection _connection;
        internal readonly HighPerfSubscription _subscription;

#if NET9_0_OR_GREATER
        private readonly Lock _eventLock = new Lock();
#else
        private readonly object _eventLock = new object();
#endif

        private bool _connectionEventsSubscribed = true;
        private readonly List<Action> _connectionClosedEventHandlers = new List<Action>();

        /// <summary>
        /// Event when the connection is closed and will not be reconnected
        /// </summary>
        public event Action ConnectionClosed
        {
            add { lock (_eventLock) _connectionClosedEventHandlers.Add(value); }
            remove { lock (_eventLock) _connectionClosedEventHandlers.Remove(value); }
        }

        /// <summary>
        /// Event when an exception happens during the handling of the data
        /// </summary>
        public event Action<Exception> Exception
        {
            add => _subscription.Exception += value;
            remove => _subscription.Exception -= value;
        }

        /// <summary>
        /// The id of the socket
        /// </summary>
        public int SocketId => _connection.SocketId;

        /// <summary>
        /// The id of the subscription
        /// </summary>
        public int Id => _subscription.Id;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connection">The socket connection the subscription is on</param>
        /// <param name="subscription">The subscription</param>
        public HighPerfUpdateSubscription(HighPerfSocketConnection connection, HighPerfSubscription subscription)
        {
            _connection = connection;
            _connection.ConnectionClosed += HandleConnectionClosedEvent;

            _subscription = subscription;
        }

        private void UnsubscribeConnectionEvents()
        {
            lock (_eventLock)
            {
                if (!_connectionEventsSubscribed)
                    return;

                _connection.ConnectionClosed -= HandleConnectionClosedEvent;
                _connectionEventsSubscribed = false;
            }
        }

        private void HandleConnectionClosedEvent()
        {
            UnsubscribeConnectionEvents();

            List<Action> handlers;
            lock (_eventLock)
                handlers = _connectionClosedEventHandlers.ToList();

            foreach(var callback in handlers)
                callback();
        }

        /// <summary>
        /// Close the subscription
        /// </summary>
        /// <returns></returns>
        public Task CloseAsync()
        {
            return _connection.CloseAsync(_subscription);
        }

        /// <summary>
        /// Unsubscribe a subscription
        /// </summary>
        /// <returns></returns>
        internal async Task UnsubscribeAsync()
        {
            await _connection.UnsubscribeAsync(_subscription).ConfigureAwait(false);
        }
    }
}
