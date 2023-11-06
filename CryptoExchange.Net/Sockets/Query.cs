using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Query 
    /// </summary>
    public abstract class BaseQuery : IMessageProcessor
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; } = ExchangeHelpers.NextId();
        /// <summary>
        /// Strings to identify this subscription with
        /// </summary>
        public abstract List<string> Identifiers { get; }

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
        /// The pending request for this query
        /// </summary>
        public BasePendingRequest? PendingRequest { get; private set; }

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
        public BasePendingRequest CreatePendingRequest()
        {
            PendingRequest = GetPendingRequest(Id);
            return PendingRequest;
        }

        /// <summary>
        /// Create a pending request for this query
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract BasePendingRequest GetPendingRequest(int id);

        /// <summary>
        /// Handle a response message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task<CallResult> HandleMessageAsync(DataEvent<BaseParsedMessage> message);

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

        /// <inheritdoc />
        public override async Task<CallResult> HandleMessageAsync(DataEvent<BaseParsedMessage> message)
        {
            await PendingRequest!.ProcessAsync(message).ConfigureAwait(false);
            return await HandleMessageAsync(message.As((ParsedMessage<TResponse>)message.Data)).ConfigureAwait(false);
        }

        /// <summary>
        /// Handle the query response
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual Task<CallResult<TResponse>> HandleMessageAsync(DataEvent<ParsedMessage<TResponse>> message) => Task.FromResult(new CallResult<TResponse>(message.Data.Data!));

        /// <inheritdoc />
        public override BasePendingRequest GetPendingRequest(int id) => PendingRequest<TResponse>.CreateForQuery(this, id);
    }
}
