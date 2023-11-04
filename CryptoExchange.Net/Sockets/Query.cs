using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Query 
    /// </summary>
    public abstract class BaseQuery
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

        public abstract bool MessageMatchesQuery(BaseParsedMessage message);
        public abstract CallResult HandleResult(BaseParsedMessage message);


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

        public abstract BasePendingRequest CreatePendingRequest();
    }
    
    public abstract class Query<TResponse> : BaseQuery
    {
        protected Query(object request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
        }

        public override CallResult HandleResult(BaseParsedMessage message) => HandleResponse((ParsedMessage<TResponse>) message);
        public override bool MessageMatchesQuery(BaseParsedMessage message) => MessageMatchesQuery((ParsedMessage<TResponse>)message);

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

        public override BasePendingRequest CreatePendingRequest() => PendingRequest<TResponse>.CreateForQuery(this);
    }
}
