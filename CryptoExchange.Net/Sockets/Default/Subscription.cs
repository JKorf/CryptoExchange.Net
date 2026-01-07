using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets.Default
{
    /// <summary>
    /// Socket subscription
    /// </summary>
    public abstract class Subscription : IMessageProcessor
    {
        /// <summary>
        /// Subscription id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Total amount of invocations
        /// </summary>
        public int TotalInvocations { get; set; }

        /// <summary>
        /// Amount of invocation during this connection
        /// </summary>
        public int ConnectionInvocations { get; set; }

        /// <summary>
        /// Is it a user subscription
        /// </summary>
        public bool UserSubscription { get; set; }

        private SubscriptionStatus _status;
        /// <summary>
        /// Current subscription status
        /// </summary>
        public SubscriptionStatus Status
        {
            get => _status;
            set 
            {
                if (_status == value)
                    return;

                _status = value;
                Task.Run(() => StatusChanged?.Invoke(value));
            }
        }

        /// <summary>
        /// Whether the subscription is active
        /// </summary>
        public bool Active => Status != SubscriptionStatus.Closing && Status != SubscriptionStatus.Closed;

        /// <summary>
        /// Whether the unsubscribing of this subscription lead to the closing of the connection
        /// </summary>
        public bool IsClosingConnection { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// If the subscription is a private subscription and needs authentication
        /// </summary>
        public bool Authenticated { get; }

        /// <summary>
        /// Matcher for this subscription
        /// </summary>
        public MessageMatcher MessageMatcher { get; set; }

        /// <summary>
        /// Router for this subscription
        /// </summary>
        public MessageRouter MessageRouter { get; set; }

        /// <summary>
        /// Cancellation token registration
        /// </summary>
        public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

        /// <summary>
        /// Exception event
        /// </summary>
        public event Action<Exception>? Exception;
        /// <summary>
        /// Listener unsubscribed event
        /// </summary>
        public event Action<SubscriptionStatus>? StatusChanged;

        /// <summary>
        /// Subscription topic
        /// </summary>
        public string? Topic { get; set; }

        /// <summary>
        /// The subscribe query for this subscription
        /// </summary>
        public Query? SubscriptionQuery { get; private set; }

        /// <summary>
        /// The unsubscribe query for this subscription
        /// </summary>
        public Query? UnsubscriptionQuery { get; private set; }

        /// <summary>
        /// The number of individual streams in this subscription
        /// </summary>
        public int IndividualSubscriptionCount { get; set; } = 1;

        /// <summary>
        /// ctor
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Subscription(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            ILogger logger,
            bool authenticated,
            bool userSubscription = true)
        {
            _logger = logger;
            Authenticated = authenticated;
            UserSubscription = userSubscription;
            Id = ExchangeHelpers.NextId();
        }

        /// <summary>
        /// Create a new subscription query
        /// </summary>
        public Query? CreateSubscriptionQuery(SocketConnection connection)
        {
            var query = GetSubQuery(connection);
            SubscriptionQuery = query;
            return query;
        }

        /// <summary>
        /// Get the subscribe query to send when subscribing
        /// </summary>
        /// <returns></returns>
        protected abstract Query? GetSubQuery(SocketConnection connection);

        /// <summary>
        /// Handle a subscription query response
        /// </summary>
        public virtual void HandleSubQueryResponse(SocketConnection connection, object? message) { }

        /// <summary>
        /// Handle an unsubscription query response
        /// </summary>
        public virtual void HandleUnsubQueryResponse(SocketConnection connection, object message) { }

        /// <summary>
        /// Create a new unsubscription query
        /// </summary>
        public Query? CreateUnsubscriptionQuery(SocketConnection connection)
        {
            var query = GetUnsubQuery(connection);
            UnsubscriptionQuery = query;
            return query;
        }

        /// <summary>
        /// Get the unsubscribe query to send when unsubscribing
        /// </summary>
        /// <returns></returns>
        protected abstract Query? GetUnsubQuery(SocketConnection connection);

        /// <inheritdoc />
        public virtual CallResult<object> Deserialize(IMessageAccessor message, Type type) => message.Deserialize(type);

        /// <summary>
        /// Handle an update message
        /// </summary>
        public CallResult Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data, MessageHandlerLink matcher)
        {
            ConnectionInvocations++;
            TotalInvocations++;
            return matcher.Handle(connection, receiveTime, originalData, data);
        }

        /// <summary>
        /// Handle an update message
        /// </summary>
        public CallResult? Handle(SocketConnection connection, DateTime receiveTime, string? originalData, object data, MessageRoute route)
        {
            ConnectionInvocations++;
            TotalInvocations++;
            return route.Handle(connection, receiveTime, originalData, data);
        }

        /// <summary>
        /// Reset the subscription
        /// </summary>
        public void Reset()
        {
            Status = SubscriptionStatus.Pending;
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

        /// <summary>
        /// State of this subscription
        /// </summary>
        /// <param name="Id">The id of the subscription</param>
        /// <param name="Status">Subscription status</param>
        /// <param name="Invocations">Number of times this subscription got a message</param>
        /// <param name="ListenMatcher">Matcher for this subscription</param>
        public record SubscriptionState(
            int Id,
            SubscriptionStatus Status,
            int Invocations,
            MessageMatcher ListenMatcher
        );

        /// <summary>
        /// Get the state of this subscription
        /// </summary>
        /// <returns></returns>
        public SubscriptionState GetState()
        {
            return new SubscriptionState(Id, Status, TotalInvocations, MessageMatcher);
        }
    }
}
