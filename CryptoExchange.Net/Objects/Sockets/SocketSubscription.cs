using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// Socket subscription
    /// </summary>
    public class SocketSubscriptionListener : IStreamMessageListener
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
        /// The request object send when subscribing on the server. Either this or the `Identifier` property should be set
        /// </summary>
        public SubscriptionActor Subscription { get; set; }

        /// <summary>
        /// Whether this is a user subscription or an internal listener
        /// </summary>
        public bool UserSubscription { get; set; }

        /// <summary>
        /// If the subscription has been confirmed to be subscribed by the server
        /// </summary>
        public bool Confirmed { get; set; }

        /// <summary>
        /// Whether authentication is needed for this subscription
        /// </summary>
        public bool Authenticated => Subscription.Authenticated;

        /// <summary>
        /// Whether we're closing this subscription and a socket connection shouldn't be kept open for it
        /// </summary>
        public bool Closed { get; set; }

        /// <summary>
        /// Cancellation token registration, should be disposed when subscription is closed. Used for closing the subscription with 
        /// a provided cancelation token
        /// </summary>
        public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="userSubscription"></param>
        public SocketSubscriptionListener(int id, SubscriptionActor request, bool userSubscription)
        {
            Id = id;
            UserSubscription = userSubscription;
            Subscription = request;
        }

        /// <summary>
        /// Invoke the exception event
        /// </summary>
        /// <param name="e"></param>
        public void InvokeExceptionHandler(Exception e)
        {
            Exception?.Invoke(e);
        }

        /// <summary>
        /// The priority of this subscription
        /// </summary>
        public int Priority => Subscription is SystemSubscription ? 50 : 1;

        /// <summary>
        /// Check if message matches the subscription
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool MessageMatches(StreamMessage message) => Subscription.MessageMatchesSubscription(message);

        /// <summary>
        /// Process the message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task ProcessAsync(StreamMessage message) => Subscription.HandleEventAsync(message);
    }
}
