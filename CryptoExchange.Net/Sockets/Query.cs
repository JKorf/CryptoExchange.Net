using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Query 
    /// </summary>
    public abstract class Query
    {
        /// <summary>
        /// The query request
        /// </summary>
        public object Request { get; set; }

        /// <summary>
        /// If this is a private request
        /// </summary>
        public bool Authenticated { get; }

        /// <summary>
        /// Weight of the query
        /// </summary>
        public int Weight { get; }

        /// <summary>
        /// Check if the message is the response to the query
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool MessageMatchesQuery(StreamMessage message);
        /// <summary>
        /// Handle the query response
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract CallResult HandleResponse(StreamMessage message);

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authenticated"></param>
        /// <param name="weight"></param>
        public Query(object request, bool authenticated, int weight = 1)
        {
            Authenticated = authenticated;
            Request = request;
            Weight = weight;
        }
    }
}
