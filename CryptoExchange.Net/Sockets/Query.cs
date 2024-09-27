using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Query 
    /// </summary>
    public abstract class Query : IMessageProcessor
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; } = ExchangeHelpers.NextId();

        /// <summary>
        /// Has this query been completed
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// The number of required responses. Can be more than 1 when for example subscribing multiple symbols streams in a single request,
        /// and each symbol receives it's own confirmation response
        /// </summary>
        public int RequiredResponses { get; set; } = 1;

        /// <summary>
        /// The current number of responses received on this query
        /// </summary>
        public int CurrentResponses { get; set; }

        /// <summary>
        /// Timestamp of when the request was send
        /// </summary>
        public DateTime RequestTimestamp { get; set; }
        
        /// <summary>
        /// Result
        /// </summary>
        public CallResult? Result { get; set; }
        
        /// <summary>
        /// Response
        /// </summary>
        public object? Response { get; set; }

        /// <summary>
        /// Wait event for the calling message processing thread
        /// </summary>
        public AsyncResetEvent? ContinueAwaiter { get; set; }

        /// <summary>
        /// Strings to match this query to a received message
        /// </summary>
        public abstract HashSet<string> ListenerIdentifiers { get; set; }

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
        /// Get the type the message should be deserialized to
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Type? GetMessageType(IMessageAccessor message);

        /// <summary>
        /// Wait event for response
        /// </summary>
        protected AsyncResetEvent _event;

        /// <summary>
        /// Cancellation token
        /// </summary>
        protected CancellationTokenSource? _cts;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authenticated"></param>
        /// <param name="weight"></param>
        public Query(object request, bool authenticated, int weight = 1)
        {
            _event = new AsyncResetEvent(false, false);

            Authenticated = authenticated;
            Request = request;
            Weight = weight;
        }

        /// <summary>
        /// Signal that the request has been send and the timeout timer should start
        /// </summary>
        public void IsSend(TimeSpan timeout)
        {
            // Start timeout countdown
            RequestTimestamp = DateTime.UtcNow;
            _cts = new CancellationTokenSource(timeout);
            _cts.Token.Register(Timeout, false);
        }

        /// <summary>
        /// Wait until timeout or the request is completed
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        public async Task WaitAsync(TimeSpan timeout, CancellationToken ct) => await _event.WaitAsync(timeout, ct).ConfigureAwait(false);

        /// <inheritdoc />
        public virtual CallResult<object> Deserialize(IMessageAccessor message, Type type) => message.Deserialize(type);

        /// <summary>
        /// Mark request as timeout
        /// </summary>
        public abstract void Timeout();

        /// <summary>
        /// Mark request as failed
        /// </summary>
        /// <param name="error"></param>
        public abstract void Fail(Error error);

        /// <summary>
        /// Handle a response message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public abstract Task<CallResult> Handle(SocketConnection connection, DataEvent<object> message);

    }

    /// <summary>
    /// Query
    /// </summary>
    /// <typeparam name="TServerResponse">The type returned from the server</typeparam>
    /// <typeparam name="THandlerResponse">The type to be returned to the caller</typeparam>
    public abstract class Query<TServerResponse, THandlerResponse> : Query
    {
        /// <inheritdoc />
        public override Type? GetMessageType(IMessageAccessor message) => typeof(TServerResponse);

        /// <summary>
        /// The typed call result
        /// </summary>
        public CallResult<THandlerResponse>? TypedResult => (CallResult<THandlerResponse>?)Result;

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
        public override async Task<CallResult> Handle(SocketConnection connection, DataEvent<object> message)
        {
            CurrentResponses++;
            if (CurrentResponses == RequiredResponses)
            {
                Completed = true;
                Response = message.Data;
            }

            if (Result?.Success != false)
                // If an error result is already set don't override that
                Result = HandleMessage(connection, message.As((TServerResponse)message.Data));

            if (CurrentResponses == RequiredResponses)
            {
                _event.Set();
                if (ContinueAwaiter != null)
                    await ContinueAwaiter.WaitAsync().ConfigureAwait(false);
            }

            return Result;
        }

        /// <summary>
        /// Handle the query response
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract CallResult<THandlerResponse> HandleMessage(SocketConnection connection, DataEvent<TServerResponse> message);

        /// <inheritdoc />
        public override void Timeout()
        {
            if (Completed)
                return;

            Completed = true;
            Result = new CallResult<THandlerResponse>(new CancellationRequestedError(null, "Query timeout", null));
            ContinueAwaiter?.Set();
            _event.Set();
        }

        /// <inheritdoc />
        public override void Fail(Error error)
        {
            Result = new CallResult<THandlerResponse>(error);
            Completed = true;
            ContinueAwaiter?.Set();
            _event.Set();
        }
    }

    /// <summary>
    /// Query
    /// </summary>
    /// <typeparam name="TResponse">Response object type</typeparam>
    public abstract class Query<TResponse> : Query<TResponse, TResponse>
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
        /// <param name="connection"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override CallResult<TResponse> HandleMessage(SocketConnection connection, DataEvent<TResponse> message) => message.ToCallResult();
    }
}
