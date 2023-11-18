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
    public abstract class BaseQuery : IMessageProcessor
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; } = ExchangeHelpers.NextId();

        public bool Completed { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public CallResult? Result { get; set; }
        public BaseParsedMessage Response { get; set; }

        protected AsyncResetEvent _event;
        protected CancellationTokenSource? _cts;

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

        public abstract Type ExpectedMessageType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authenticated"></param>
        /// <param name="weight"></param>
        public BaseQuery(object request, bool authenticated, int weight = 1)
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
        /// Wait untill timeout or the request is competed
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task WaitAsync(TimeSpan timeout) => await _event.WaitAsync(timeout).ConfigureAwait(false);

        /// <summary>
        /// Mark request as timeout
        /// </summary>
        public abstract void Timeout();

        /// <summary>
        /// Mark request as failed
        /// </summary>
        /// <param name="error"></param>
        public abstract void Fail(string error);

        /// <summary>
        /// Handle a response message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task<CallResult> HandleMessageAsync(SocketConnection connection, DataEvent<BaseParsedMessage> message);

    }

    /// <summary>
    /// Query
    /// </summary>
    /// <typeparam name="TResponse">Response object type</typeparam>
    public abstract class Query<TResponse> : BaseQuery
    {
        public override Type ExpectedMessageType => typeof(TResponse);

        /// <summary>
        /// The typed call result
        /// </summary>
        public CallResult<TResponse>? TypedResult => (CallResult<TResponse>?)Result;

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
        public override async Task<CallResult> HandleMessageAsync(SocketConnection connection, DataEvent<BaseParsedMessage> message)
        {
            Completed = true;
            Response = message.Data;
            Result = await HandleMessageAsync(connection, message.As((ParsedMessage<TResponse>)message.Data)).ConfigureAwait(false);
            _event.Set();
            return Result;
        }

        /// <summary>
        /// Handle the query response
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual Task<CallResult<TResponse>> HandleMessageAsync(SocketConnection connection, DataEvent<ParsedMessage<TResponse>> message) => Task.FromResult(new CallResult<TResponse>(message.Data.TypedData!));

        /// <inheritdoc />
        public override void Timeout()
        {
            if (Completed)
                return;

            Completed = true;
            Result = new CallResult<TResponse>(new CancellationRequestedError());
            _event.Set();
        }

        /// <inheritdoc />
        public override void Fail(string error)
        {
            Result = new CallResult<TResponse>(new ServerError(error));
            Completed = true;
            _event.Set();
        }
    }
}
