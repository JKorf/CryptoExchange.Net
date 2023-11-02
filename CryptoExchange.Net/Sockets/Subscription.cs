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
        /// Check if the message is the response to the subscribe request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool MessageMatchesSubRequest(ParsedMessage message);
        public abstract CallResult HandleSubResponse(ParsedMessage message);

        /// <summary>
        /// Get the unsubscribe object to send when unsubscribing
        /// </summary>
        /// <returns></returns>
        public abstract object? GetUnsubRequest();
        /// <summary>
        /// Check if the message is the response to the unsubscribe request
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool MessageMatchesUnsubRequest(ParsedMessage message);
        public abstract CallResult HandleUnsubResponse(ParsedMessage message);

        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task HandleEventAsync(DataEvent<ParsedMessage> message);

        /// <summary>
        /// Invoke the exception event
        /// </summary>
        /// <param name="e"></param>
        public void InvokeExceptionHandler(Exception e)
        {
            Exception?.Invoke(e);
        }
    }
}
