using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        /// Logger
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// If the subscription is a private subscription and needs authentication
        /// </summary>
        public bool Authenticated { get; }

        /// <summary>
        /// Strings to match this subscription to a received message
        /// </summary>
        public abstract HashSet<string> ListenerIdentifiers { get; set; }

        /// <summary>
        /// Cancellation token registration
        /// </summary>
        public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

        /// <summary>
        /// Exception event
        /// </summary>
        public event Action<Exception>? Exception;

        /// <summary>
        /// Get the deserialization type for this message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Type? GetMessageType(IMessageAccessor message);

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticated"></param>
        /// <param name="userSubscription"></param>
        public Subscription(ILogger logger, bool authenticated, bool userSubscription = true)
        {
            _logger = logger;
            Authenticated = authenticated;
            UserSubscription = userSubscription;
            Id = ExchangeHelpers.NextId();
        }

        /// <summary>
        /// Get the subscribe query to send when subscribing
        /// </summary>
        /// <returns></returns>
        public abstract Query? GetSubQuery(SocketConnection connection);

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
        /// Get the unsubscribe query to send when unsubscribing
        /// </summary>
        /// <returns></returns>
        public abstract Query? GetUnsubQuery();

        /// <inheritdoc />
        public virtual CallResult<object> Deserialize(IMessageAccessor message, Type type) => message.Deserialize(type);

        /// <summary>
        /// Handle an update message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<CallResult> Handle(SocketConnection connection, DataEvent<object> message)
        {
            ConnectionInvocations++;
            TotalInvocations++;
            return Task.FromResult(DoHandleMessage(connection, message));
        }

        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract CallResult DoHandleMessage(SocketConnection connection, DataEvent<object> message);

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
        /// <param name="Identifiers">Identifiers the subscription is listening to</param>
        public record SubscriptionState(
            int Id,
            bool Confirmed,
            int Invocations,
            HashSet<string> Identifiers
        );

        /// <summary>
        /// Get the state of this subscription
        /// </summary>
        /// <returns></returns>
        public SubscriptionState GetState()
        {
            return new SubscriptionState(Id, Confirmed, TotalInvocations, ListenerIdentifiers);
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
