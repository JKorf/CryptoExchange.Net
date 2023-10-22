using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Query 
    /// </summary>
    public abstract class QueryActor
    {
        /// <summary>
        /// The query request
        /// </summary>
        public object Query { get; set; }

        /// <summary>
        /// If this is a private request
        /// </summary>
        public bool Authenticated { get; }

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
        /// <param name="query"></param>
        /// <param name="authenticated"></param>
        public QueryActor(object query, bool authenticated)
        {
            Authenticated = authenticated;
            Query = query;
        }
    }
}
