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
    /// Subscription base
    /// </summary>
    public abstract class Subscription
    {
        public int Id { get; set; }

        public bool UserSubscription { get; set; }
        public bool Confirmed { get; set; }
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
        public Subscription(ILogger logger, bool authenticated)
        {
            _logger = logger;
            Authenticated = authenticated;
        }

        /// <summary>
        /// Get the subscribe object to send when subscribing
        /// </summary>
        /// <returns></returns>
        public abstract object? GetSubRequest();

        /// <summary>
        /// Get the unsubscribe object to send when unsubscribing
        /// </summary>
        /// <returns></returns>
        public abstract object? GetUnsubRequest();

        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task HandleEventAsync(DataEvent<BaseParsedMessage> message);
        public abstract CallResult HandleSubResponse(BaseParsedMessage message);
        public abstract CallResult HandleUnsubResponse(BaseParsedMessage message);

        public abstract bool MessageMatchesUnsubRequest(BaseParsedMessage message);
        public abstract bool MessageMatchesSubRequest(BaseParsedMessage message);

        /// <summary>
        /// Invoke the exception event
        /// </summary>
        /// <param name="e"></param>
        public void InvokeExceptionHandler(Exception e)
        {
            Exception?.Invoke(e);
        }
    }

    public abstract class Subscription<TQuery, TEvent> : Subscription<TQuery, TEvent, TQuery>
    {
        protected Subscription(ILogger logger, bool authenticated) : base(logger, authenticated)
        {
        }
    }

    public abstract class Subscription<TSubResponse, TEvent, TUnsubResponse> : Subscription
    {
        protected Subscription(ILogger logger, bool authenticated) : base(logger, authenticated)
        {
        }

        public override CallResult HandleUnsubResponse(BaseParsedMessage message)
            => HandleUnsubResponse((ParsedMessage<TUnsubResponse>)message);

        public override CallResult HandleSubResponse(BaseParsedMessage message)
            => HandleSubResponse((ParsedMessage<TSubResponse>)message);

        public override Task HandleEventAsync(DataEvent<BaseParsedMessage> message)
            => HandleEventAsync(message.As((ParsedMessage<TEvent>)message.Data));

        public override bool MessageMatchesSubRequest(BaseParsedMessage message)
            => MessageMatchesSubRequest((ParsedMessage<TSubResponse>)message);

        public override bool MessageMatchesUnsubRequest(BaseParsedMessage message)
            => MessageMatchesUnsubRequest((ParsedMessage<TUnsubResponse>)message);

        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task HandleEventAsync(DataEvent<ParsedMessage<TEvent>> message);

        /// <summary>
        /// Check if the message is the response to the subscribe request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool MessageMatchesSubRequest(ParsedMessage<TSubResponse> message);
        public abstract CallResult HandleSubResponse(ParsedMessage<TSubResponse> message);

        /// <summary>
        /// Check if the message is the response to the unsubscribe request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool MessageMatchesUnsubRequest(ParsedMessage<TUnsubResponse> message);
        public abstract CallResult HandleUnsubResponse(ParsedMessage<TUnsubResponse> message);
    }
}
