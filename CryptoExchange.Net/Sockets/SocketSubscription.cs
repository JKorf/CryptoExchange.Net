using System;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.Sockets
{
    public class SocketSubscription
    {
        public event Action<Exception> Exception;

        /// <summary>
        /// Message handlers for this subscription. Should return true if the message is handled and should not be distributed to the other handlers
        /// </summary>
        public Action<SocketConnection, JToken> MessageHandler { get; set; }

        public object Request { get; set; }
        public string Identifier { get; set; }
        public bool UserSubscription { get; set; }
        
        public bool Confirmed { get; set; }


        public SocketSubscription(string identifier, object request, bool userSubscription, Action<SocketConnection, JToken> dataHandler)
        {
            UserSubscription = userSubscription;
            MessageHandler = dataHandler;
            Identifier = identifier;
            Request = request;
        }
       
        public void InvokeExceptionHandler(Exception e)
        {
            Exception?.Invoke(e);
        }
    }
}
