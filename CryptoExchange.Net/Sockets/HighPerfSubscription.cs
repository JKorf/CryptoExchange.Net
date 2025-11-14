using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Socket subscription
    /// </summary>
    public abstract class HighPerfSubscription
    {
        /// <summary>
        /// Subscription id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Total amount of invocations
        /// </summary>
        public int TotalInvocations { get; set; }

        //private SubscriptionStatus _status;
        ///// <summary>
        ///// Current subscription status
        ///// </summary>
        //public SubscriptionStatus Status
        //{
        //    get => _status;
        //    set 
        //    {
        //        if (_status == value)
        //            return;

        //        _status = value;
        //        Task.Run(() => StatusChanged?.Invoke(value));
        //    }
        //}

        ///// <summary>
        ///// Whether the subscription is active
        ///// </summary>
        //public bool Active => Status != SubscriptionStatus.Closing && Status != SubscriptionStatus.Closed;

        ///// <summary>
        ///// Whether the unsubscribing of this subscription lead to the closing of the connection
        ///// </summary>
        //public bool IsClosingConnection { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Cancellation token registration
        /// </summary>
        public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

        /// <summary>
        /// Exception event
        /// </summary>
        public event Action<Exception>? Exception;
        ///// <summary>
        ///// Listener unsubscribed event
        ///// </summary>
        //public event Action<SubscriptionStatus>? StatusChanged;

        /// <summary>
        /// The subscribe query for this subscription
        /// </summary>
        public Query? SubscriptionQuery { get; private set; }

        /// <summary>
        /// The unsubscribe query for this subscription
        /// </summary>
        public Query? UnsubscriptionQuery { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        public HighPerfSubscription(ILogger logger)
        {
            _logger = logger;
            Id = ExchangeHelpers.NextId();
        }

        /// <summary>
        /// Create a new subscription query
        /// </summary>
        public Query? CreateSubscriptionQuery(HighPerfSocketConnection connection)
        {
            var query = GetSubQuery(connection);
            SubscriptionQuery = query;
            return query;
        }

        /// <summary>
        /// Get the subscribe query to send when subscribing
        /// </summary>
        /// <returns></returns>
        protected abstract Query? GetSubQuery(HighPerfSocketConnection connection);

        /// <summary>
        /// Create a new unsubscription query
        /// </summary>
        public Query? CreateUnsubscriptionQuery(HighPerfSocketConnection connection)
        {
            var query = GetUnsubQuery(connection);
            UnsubscriptionQuery = query;
            return query;
        }

        /// <summary>
        /// Get the unsubscribe query to send when unsubscribing
        /// </summary>
        /// <returns></returns>
        protected abstract Query? GetUnsubQuery(HighPerfSocketConnection connection);

        /// <summary>
        /// Reset the subscription
        /// </summary>
        public void Reset()
        {
            DoHandleReset();
        }

        /// <summary>
        /// Connection has been reset, do any logic for resetting the subscription
        /// </summary>
        public virtual void DoHandleReset() { }

        /// <summary>
        /// Invoke the exception event
        /// </summary>
        /// <param name="e"></param>
        public void InvokeExceptionHandler(Exception e)
        {
            Exception?.Invoke(e);
        }

    }

    /// <inheritdoc />
    public abstract class HighPerfSubscription<TUpdateType> : HighPerfSubscription
    {
        private Func<TUpdateType, ValueTask> _handler;

        /// <summary>
        /// ctor
        /// </summary>
        protected HighPerfSubscription(ILogger logger, Func<TUpdateType, ValueTask> handler) : base(logger)
        {
            _handler = handler;
        }

        public ValueTask HandleAsync(TUpdateType update)
        {
            return _handler.Invoke(update);
        }
    }
}
