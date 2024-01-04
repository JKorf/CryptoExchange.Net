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
        /// Strings to identify this subscription with
        /// </summary>
        public abstract List<string> StreamIdentifiers { get; set; }

        /// <summary>
        /// Cancellation token registration
        /// </summary>
        public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

        /// <summary>
        /// Exception event
        /// </summary>
        public event Action<Exception>? Exception;

        public abstract Dictionary<string, Type> TypeMapping { get; }

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
        /// Get the subscribe object to send when subscribing
        /// </summary>
        /// <returns></returns>
        public abstract BaseQuery? GetSubQuery(SocketConnection connection);

        public virtual void HandleSubQueryResponse(BaseParsedMessage message) { }
        public virtual void HandleUnsubQueryResponse(BaseParsedMessage message) { }

        /// <summary>
        /// Get the unsubscribe object to send when unsubscribing
        /// </summary>
        /// <returns></returns>
        public abstract BaseQuery? GetUnsubQuery();

        public async Task<CallResult> HandleMessageAsync(SocketConnection connection, DataEvent<BaseParsedMessage> message)
        {
            ConnectionInvocations++;
            TotalInvocations++;
            return await DoHandleMessageAsync(connection, message).ConfigureAwait(false);
        }

        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task<CallResult> DoHandleMessageAsync(SocketConnection connection, DataEvent<BaseParsedMessage> message);

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
    public abstract class Subscription<TQuery> : Subscription<TQuery, TQuery>
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticated"></param>
        protected Subscription(ILogger logger, bool authenticated) : base(logger, authenticated)
        {
        }
    }

    /// <inheritdoc />
    public abstract class Subscription<TSubResponse, TUnsubResponse> : Subscription
    {
        //public override Func<string, Type> ExpectedTypeDelegate => (x) => typeof(TEvent);

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="authenticated"></param>
        protected Subscription(ILogger logger, bool authenticated) : base(logger, authenticated)
        {
        }

        /// <inheritdoc />
        //public override Task<CallResult> DoHandleMessageAsync(SocketConnection connection, DataEvent<BaseParsedMessage> message)
        //    => HandleEventAsync(connection, message.As((ParsedMessage<TEvent>)message.Data));

        public override void HandleSubQueryResponse(BaseParsedMessage message)
            => HandleSubQueryResponse((ParsedMessage<TSubResponse>)message);

        public virtual void HandleSubQueryResponse(ParsedMessage<TSubResponse> message) { }

        public override void HandleUnsubQueryResponse(BaseParsedMessage message)
            => HandleUnsubQueryResponse((ParsedMessage<TUnsubResponse>)message);

        public virtual void HandleUnsubQueryResponse(ParsedMessage<TUnsubResponse> message) { }

    }
}
