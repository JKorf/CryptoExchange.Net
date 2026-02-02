using CryptoExchange.Net.Sockets.Default;
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
    public class UpdateSubscription
    {
        private readonly SocketConnection _connection;
        internal readonly Subscription _subscription;

#if NET9_0_OR_GREATER
        private readonly Lock _eventLock = new Lock();
#else
        private readonly object _eventLock = new object();
#endif

        private bool _connectionEventsSubscribed = true;
        private List<Action> _connectionClosedEventHandlers = new List<Action>();
        private List<Action> _connectionLostEventHandlers = new List<Action>();
        private List<Action<Error>> _resubscribeFailedEventHandlers = new List<Action<Error>>();
        private List<Action<TimeSpan>> _connectionRestoredEventHandlers = new List<Action<TimeSpan>>();
        private List<Action> _activityPausedEventHandlers = new List<Action>();
        private List<Action> _activityUnpausedEventHandlers = new List<Action>();

        /// <summary>
        /// Event when the status of the subscription changes
        /// </summary>
        public event Action<SubscriptionStatus>? SubscriptionStatusChanged;

        /// <summary>
        /// Event when the connection is lost. The socket will automatically reconnect when possible.
        /// </summary>
        public event Action ConnectionLost
        {
            add { lock (_eventLock) _connectionLostEventHandlers.Add(value); }
            remove { lock (_eventLock) _connectionLostEventHandlers.Remove(value); }
        }

        /// <summary>
        /// Event when the connection is closed and will not be reconnected
        /// </summary>
        public event Action ConnectionClosed
        {
            add { lock (_eventLock) _connectionClosedEventHandlers.Add(value); }
            remove { lock (_eventLock) _connectionClosedEventHandlers.Remove(value); }
        }

        /// <summary>
        /// Event when a lost connection is restored, but the resubscribing of update subscriptions failed
        /// </summary>
        public event Action<Error> ResubscribingFailed
        {
            add { lock (_eventLock) _resubscribeFailedEventHandlers.Add(value); }
            remove { lock (_eventLock) _resubscribeFailedEventHandlers.Remove(value); }
        }

        /// <summary>
        /// Event when the connection is restored. Timespan parameter indicates the time the socket has been offline for before reconnecting. 
        /// Note that when the executing code is suspended and resumed at a later period (for example, a laptop going to sleep) the disconnect time will be incorrect as the disconnect
        /// will only be detected after resuming the code, so the initial disconnect time is lost. Use the timespan only for informational purposes.
        /// </summary>
        public event Action<TimeSpan> ConnectionRestored
        {
            add { lock (_eventLock) _connectionRestoredEventHandlers.Add(value); }
            remove { lock (_eventLock) _connectionRestoredEventHandlers.Remove(value); }
        }

        /// <summary>
        /// Event when the connection to the server is paused based on a server indication. No operations can be performed while paused
        /// </summary>
        public event Action ActivityPaused
        {
            add { lock (_eventLock) _activityPausedEventHandlers.Add(value); }
            remove { lock (_eventLock) _activityPausedEventHandlers.Remove(value); }
        }

        /// <summary>
        /// Event when the connection to the server is unpaused after being paused
        /// </summary>
        public event Action ActivityUnpaused
        {
            add { lock (_eventLock) _activityUnpausedEventHandlers.Add(value); }
            remove { lock (_eventLock) _activityUnpausedEventHandlers.Remove(value); }
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
        /// The last timestamp anything was received from the server
        /// </summary>
        public DateTime? LastReceiveTime => _connection.LastReceiveTime;

        /// <summary>
        /// The current websocket status
        /// </summary>
        public SocketStatus Status => _connection.Status;

        /// <summary>
        /// The current subscription status
        /// </summary>
        public SubscriptionStatus SubscriptionStatus => _subscription.Status;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connection">The socket connection the subscription is on</param>
        /// <param name="subscription">The subscription</param>
        public UpdateSubscription(SocketConnection connection, Subscription subscription)
        {
            _connection = connection;
            _connection.ConnectionClosed += HandleConnectionClosedEvent;
            _connection.ConnectionLost += HandleConnectionLostEvent;
            _connection.ConnectionRestored += HandleConnectionRestoredEvent;
            _connection.ResubscribingFailed += HandleResubscribeFailedEvent;
            _connection.ActivityPaused += HandlePausedEvent;
            _connection.ActivityUnpaused += HandleUnpausedEvent;

            _subscription = subscription;
            _subscription.StatusChanged += (x) => SubscriptionStatusChanged?.Invoke(x);
        }

        private void UnsubscribeConnectionEvents()
        {
            lock (_eventLock)
            {
                if (!_connectionEventsSubscribed)
                    return;

                _connection.ConnectionClosed -= HandleConnectionClosedEvent;
                _connection.ConnectionLost -= HandleConnectionLostEvent;
                _connection.ConnectionRestored -= HandleConnectionRestoredEvent;
                _connection.ResubscribingFailed -= HandleResubscribeFailedEvent;
                _connection.ActivityPaused -= HandlePausedEvent;
                _connection.ActivityUnpaused -= HandleUnpausedEvent;
                _connectionEventsSubscribed = false;
            }
        }

        private void HandleConnectionClosedEvent()
        {
            UnsubscribeConnectionEvents();

            // If we're not the subscription closing this connection don't bother emitting
            if (!_subscription.IsClosingConnection)
                return;

            List<Action> handlers;
            lock (_eventLock)
                handlers = _connectionClosedEventHandlers.ToList();

            foreach(var callback in handlers)
                callback();
        }

        private void HandleConnectionLostEvent()
        {
            if (!_subscription.Active)
            {
                UnsubscribeConnectionEvents();
                return;
            }

            List<Action> handlers;
            lock (_eventLock)
                handlers = _connectionLostEventHandlers.ToList();

            foreach (var callback in handlers)
                callback();
        }

        private void HandleConnectionRestoredEvent(TimeSpan period)
        {
            if (!_subscription.Active)
            {
                UnsubscribeConnectionEvents();
                return;
            }

            List<Action<TimeSpan>> handlers;
            lock (_eventLock)
                handlers = _connectionRestoredEventHandlers.ToList();

            foreach (var callback in handlers)
                callback(period);
        }

        private void HandleResubscribeFailedEvent(Error error)
        {
            if (!_subscription.Active)
            {
                UnsubscribeConnectionEvents();
                return;
            }

            List<Action<Error>> handlers;
            lock (_eventLock)
                handlers = _resubscribeFailedEventHandlers.ToList();

            foreach (var callback in handlers)
                callback(error);
        }

        private void HandlePausedEvent()
        {
            if (!_subscription.Active)
            {
                UnsubscribeConnectionEvents();
                return;
            }

            List<Action> handlers;
            lock (_eventLock)
                handlers = _activityPausedEventHandlers.ToList();

            foreach (var callback in handlers)
                callback();
        }

        private void HandleUnpausedEvent()
        {
            if (!_subscription.Active)
            {
                UnsubscribeConnectionEvents();
                return;
            }

            List<Action> handlers;
            lock (_eventLock)
                handlers = _activityUnpausedEventHandlers.ToList();

            foreach (var callback in handlers)
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
        /// Close the socket to cause a reconnect
        /// </summary>
        /// <returns></returns>
        public Task ReconnectAsync()
        {
            return _connection.TriggerReconnectAsync();
        }

        /// <summary>
        /// Unsubscribe a subscription
        /// </summary>
        /// <returns></returns>
        internal async Task UnsubscribeAsync()
        {
            await _connection.UnsubscribeAsync(_subscription).ConfigureAwait(false);
        }

        /// <summary>
        /// Resubscribe this subscription
        /// </summary>
        /// <returns></returns>
        internal async Task<CallResult> ResubscribeAsync()
        {
            return await _connection.ResubscribeAsync(_subscription).ConfigureAwait(false);
        }
    }
}
