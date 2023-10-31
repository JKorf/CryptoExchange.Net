using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Subscription base
    /// </summary>
    public abstract class Subscription
    {
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
        public abstract (bool, CallResult?) MessageMatchesSubRequest(ParsedMessage message);

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
        public abstract (bool, CallResult?) MessageMatchesUnsubRequest(ParsedMessage message);

        /// <summary>
        /// Handle the update message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task HandleEventAsync(DataEvent<ParsedMessage> message);
    }
}
