using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// Controller for an update subscription which isn't backed by a websocket connection. Can be used for testing.
    /// </summary>
    public class ManualUpdateSubscription
    {
        private readonly Func<Task> _closeAsync;
        private readonly Func<Task> _reconnectAsync;
        private readonly Func<Task<CallResult>> _resubscribeAsync;
        private readonly ManualSubscription _manualSubscription;
        private int _closedEventInvoked;

        /// <summary>
        /// The update subscription
        /// </summary>
        public UpdateSubscription Subscription { get; }

        /// <summary>
        /// The virtual socket id
        /// </summary>
        public int SocketId { get; }

        /// <summary>
        /// The last timestamp anything was received by the subscription
        /// </summary>
        public DateTime? LastReceiveTime { get; private set; }

        /// <summary>
        /// The current virtual websocket status
        /// </summary>
        public SocketStatus SocketStatus { get; private set; }

        /// <summary>
        /// Create a manually controlled update subscription
        /// </summary>
        /// <param name="socketId">The virtual socket id</param>
        /// <param name="closeAsync">Callback when the subscription is closed</param>
        /// <param name="reconnectAsync">Callback when a reconnect is requested</param>
        /// <param name="resubscribeAsync">Callback when a resubscribe is requested</param>
        public ManualUpdateSubscription(
            int socketId = 0,
            Func<Task>? closeAsync = null,
            Func<Task>? reconnectAsync = null,
            Func<Task<CallResult>>? resubscribeAsync = null)
        {
            SocketId = socketId;
            SocketStatus = SocketStatus.Connected;
            _closeAsync = closeAsync ?? (() => Task.CompletedTask);
            _reconnectAsync = reconnectAsync ?? (() => Task.CompletedTask);
            _resubscribeAsync = resubscribeAsync ?? (() => Task.FromResult(CallResult.Ok()));

            _manualSubscription = new ManualSubscription();
            _manualSubscription.Status = SubscriptionStatus.Subscribed;
            Subscription = new UpdateSubscription(this, _manualSubscription);
        }

        /// <summary>
        /// Set the last timestamp anything was received by the subscription
        /// </summary>
        /// <param name="timestamp">The receive timestamp</param>
        public void SetLastReceiveTime(DateTime? timestamp)
        {
            LastReceiveTime = timestamp;
        }

        /// <summary>
        /// Set the virtual websocket status
        /// </summary>
        /// <param name="status">The status</param>
        public void SetSocketStatus(SocketStatus status)
        {
            SocketStatus = status;
        }

        /// <summary>
        /// Set the subscription status
        /// </summary>
        /// <param name="status">The status</param>
        public void SetSubscriptionStatus(SubscriptionStatus status)
        {
            _manualSubscription.Status = status;
        }

        /// <summary>
        /// Invoke the connection lost event
        /// </summary>
        public void InvokeConnectionLost()
        {
            Subscription.HandleConnectionLostEvent();
        }

        /// <summary>
        /// Invoke the connection restored event
        /// </summary>
        /// <param name="disconnectedPeriod">The period the connection was disconnected</param>
        public void InvokeConnectionRestored(TimeSpan disconnectedPeriod)
        {
            Subscription.HandleConnectionRestoredEvent(disconnectedPeriod);
        }

        /// <summary>
        /// Invoke the connection closed event
        /// </summary>
        public void InvokeConnectionClosed()
        {
            if (Interlocked.Exchange(ref _closedEventInvoked, 1) != 0)
                return;

            SocketStatus = SocketStatus.Closed;
            _manualSubscription.Status = SubscriptionStatus.Closed;
            Subscription.HandleConnectionClosedEvent();
        }

        /// <summary>
        /// Invoke the resubscribing failed event
        /// </summary>
        /// <param name="error">The resubscribe error</param>
        public void InvokeResubscribingFailed(Error error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            Subscription.HandleResubscribeFailedEvent(error);
        }

        /// <summary>
        /// Invoke the activity paused event
        /// </summary>
        public void InvokeActivityPaused()
        {
            Subscription.HandlePausedEvent();
        }

        /// <summary>
        /// Invoke the activity unpaused event
        /// </summary>
        public void InvokeActivityUnpaused()
        {
            Subscription.HandleUnpausedEvent();
        }

        /// <summary>
        /// Invoke the exception event
        /// </summary>
        /// <param name="exception">The exception</param>
        public void InvokeException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            _manualSubscription.InvokeExceptionHandler(exception);
        }

        internal async Task CloseAsync()
        {
            if (_manualSubscription.Status == SubscriptionStatus.Closed
                || _manualSubscription.Status == SubscriptionStatus.Closing)
                return;

            _manualSubscription.Status = SubscriptionStatus.Closing;
            try
            {
                await _closeAsync().ConfigureAwait(false);
            }
            finally
            {
                _manualSubscription.Status = SubscriptionStatus.Closed;
            }
        }

        internal Task ReconnectAsync()
        {
            return _reconnectAsync();
        }

        internal Task<CallResult> ResubscribeAsync()
        {
            return _resubscribeAsync();
        }

        private class ManualSubscription : Subscription
        {
            public ManualSubscription()
                : base(NullLogger.Instance, false)
            {
                MessageRouter = MessageRouter.Create();
            }

            protected override Query? GetSubQuery(SocketConnection connection)
            {
                return null;
            }

            protected override Query? GetUnsubQuery(SocketConnection connection)
            {
                return null;
            }
        }
    }
}
