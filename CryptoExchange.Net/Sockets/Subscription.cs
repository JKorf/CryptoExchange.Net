using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
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
        
        /// <summary>
        /// Has the subscription been confirmed
        /// </summary>
        public bool Confirmed { get; set; }

        /// <summary>
        /// Is the subscription closed
        /// </summary>
        public bool Closed { get; set; }

        /// <summary>
        /// Is the subscription currently resubscribing
        /// </summary>
        public bool IsResubscribing { get; set; }

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
        public MessageMatcher MessageMatcher { get; set; } = null!;

        /// <summary>
        /// Cancellation token registration
        /// </summary>
        public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

        /// <summary>
        /// Exception event
        /// </summary>
        public event Action<Exception>? Exception;

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
        /// ctor
        /// </summary>
        public Subscription(ILogger logger, bool authenticated, bool userSubscription = true)
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
        /// <param name="message"></param>
        public virtual void HandleSubQueryResponse(object message) { }

        /// <summary>
        /// Handle an unsubscription query response
        /// </summary>
        /// <param name="message"></param>
        public virtual void HandleUnsubQueryResponse(object message) { }

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
        public Task<CallResult> Handle(SocketConnection connection, DataEvent<object> message, MessageHandlerLink matcher)
        {
            ConnectionInvocations++;
            TotalInvocations++;
            return Task.FromResult(matcher.Handle(connection, message));
        }

        /// <summary>
        /// Reset the subscription
        /// </summary>
        public void Reset()
        {
            Confirmed = false;
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
        /// <param name="Confirmed">True when the subscription query is handled (either accepted or rejected)</param>
        /// <param name="Invocations">Number of times this subscription got a message</param>
        /// <param name="ListenMatcher">Matcher for this subscription</param>
        public record SubscriptionState(
            int Id,
            bool Confirmed,
            int Invocations,
            MessageMatcher ListenMatcher
        );

        /// <summary>
        /// Get the state of this subscription
        /// </summary>
        /// <returns></returns>
        public SubscriptionState GetState()
        {
            return new SubscriptionState(Id, Confirmed, TotalInvocations, MessageMatcher);
        }
    }

    /// <inheritdoc />
    public abstract class Subscription<TSubResponse, TUnsubResponse> : Subscription
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticated"></param>
        protected Subscription(ILogger logger, bool authenticated) : base(logger, authenticated)
        {
        }

        /// <inheritdoc />
        public override void HandleSubQueryResponse(object message)
            => HandleSubQueryResponse((TSubResponse)message);

        /// <summary>
        /// Handle a subscription query response
        /// </summary>
        /// <param name="message"></param>
        public virtual void HandleSubQueryResponse(TSubResponse message) { }

        /// <inheritdoc />
        public override void HandleUnsubQueryResponse(object message)
            => HandleUnsubQueryResponse((TUnsubResponse)message);

        /// <summary>
        /// Handle an unsubscription query response
        /// </summary>
        /// <param name="message"></param>
        public virtual void HandleUnsubQueryResponse(TUnsubResponse message) { }

    }
}
