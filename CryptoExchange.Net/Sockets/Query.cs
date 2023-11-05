using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Query 
    /// </summary>
    public abstract class BaseQuery
    {
        /// <summary>
        /// The query request object
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
        /// ctor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authenticated"></param>
        /// <param name="weight"></param>
        public BaseQuery(object request, bool authenticated, int weight = 1)
        {
            Authenticated = authenticated;
            Request = request;
            Weight = weight;
        }

        /// <summary>
        /// Create a pending request for this query
        /// </summary>
        public abstract BasePendingRequest CreatePendingRequest();
    }
    
    /// <summary>
    /// Query
    /// </summary>
    /// <typeparam name="TResponse">Response object type</typeparam>
    public abstract class Query<TResponse> : BaseQuery
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authenticated"></param>
        /// <param name="weight"></param>
        protected Query(object request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
        }

        /// <summary>
        /// Handle the query response
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract CallResult<TResponse> HandleResponse(ParsedMessage<TResponse> message);

        /// <summary>
        /// Check if the message is the response to the query
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract bool MessageMatchesQuery(ParsedMessage<TResponse> message);

        /// <inheritdoc />
        public override BasePendingRequest CreatePendingRequest() => PendingRequest<TResponse>.CreateForQuery(this);
    }
}
