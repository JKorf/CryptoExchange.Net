using System;
using System.Threading;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Socket subscription
    /// </summary>
    public class SocketSubscription
    {
        /// <summary>
        /// Unique subscription id
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Exception event
        /// </summary>
        public event Action<Exception>? Exception;

        /// <summary>
        /// Message handlers for this subscription. Should return true if the message is handled and should not be distributed to the other handlers
        /// </summary>
        public Action<MessageEvent> MessageHandler { get; set; }

        /// <summary>
        /// The request object send when subscribing on the server. Either this or the `Identifier` property should be set
        /// </summary>
        public object? Request { get; set; }

        /// <summary>
        /// The subscription identifier, used instead of a `Request` object to identify the subscription
        /// </summary>
        public string? Identifier { get; set; }

        /// <summary>
        /// Whether this is a user subscription or an internal listener
        /// </summary>
        public bool UserSubscription { get; set; }
        
        /// <summary>
        /// If the subscription has been confirmed to be subscribed by the server
        /// </summary>
        public bool Confirmed { get; set; }

        /// <summary>
        /// Cancellation token registration, should be disposed when subscription is closed. Used for closing the subscription with 
        /// a provided cancelation token
        /// </summary>
        public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

        private SocketSubscription(int id, object? request, string? identifier, bool userSubscription, Action<MessageEvent> dataHandler)
        {
            Id = id;
            UserSubscription = userSubscription;
            MessageHandler = dataHandler;
            Request = request;
            Identifier = identifier;
        }

        /// <summary>
        /// Create SocketSubscription for a subscribe request
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="userSubscription"></param>
        /// <param name="dataHandler"></param>
        /// <returns></returns>
        public static SocketSubscription CreateForRequest(int id, object request, bool userSubscription,
            Action<MessageEvent> dataHandler)
        {
            return new SocketSubscription(id, request, null, userSubscription, dataHandler);
        }

        /// <summary>
        /// Create SocketSubscription for an identifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="identifier"></param>
        /// <param name="userSubscription"></param>
        /// <param name="dataHandler"></param>
        /// <returns></returns>
        public static SocketSubscription CreateForIdentifier(int id, string identifier, bool userSubscription,
            Action<MessageEvent> dataHandler)
        {
            return new SocketSubscription(id, null, identifier, userSubscription, dataHandler);
        }

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
