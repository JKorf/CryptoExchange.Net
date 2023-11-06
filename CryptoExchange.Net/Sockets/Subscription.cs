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
        public abstract List<string> Identifiers { get; }

        /// <summary>
        /// Cancellation token registration
        /// </summary>
        public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

        /// <summary>
        /// Exception event
        /// </summary>
        public event Action<Exception>? Exception;

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
        public abstract BaseQuery? GetSubQuery();

        /// <summary>
        /// Get the unsubscribe object to send when unsubscribing
        /// </summary>
        /// <returns></returns>
        public abstract BaseQuery? GetUnsubQuery();

        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task<CallResult> HandleMessageAsync(DataEvent<BaseParsedMessage> message);

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
    public abstract class Subscription<TQuery, TEvent> : Subscription<TQuery, TEvent, TQuery>
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
    public abstract class Subscription<TSubResponse, TEvent, TUnsubResponse> : Subscription
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
        public override Task<CallResult> HandleMessageAsync(DataEvent<BaseParsedMessage> message)
            => HandleEventAsync(message.As((ParsedMessage<TEvent>)message.Data));

        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task<CallResult> HandleEventAsync(DataEvent<ParsedMessage<TEvent>> message);
    }
}
