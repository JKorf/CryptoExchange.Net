using CryptoExchange.Net.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects.Sockets
{
    /// <summary>
    /// Pending socket request
    /// </summary>
    public abstract class BasePendingRequest
    {
        /// <summary>
        /// Request id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// If the request is completed
        /// </summary>
        public bool Completed { get; protected set; }
        /// <summary>
        /// The response object type
        /// </summary>
        public abstract Type? ResponseType { get; }
        /// <summary>
        /// Timer event
        /// </summary>
        public DateTime RequestTimestamp { get; set; }
        /// <summary>
        /// The request object
        /// </summary>
        public object Request { get; set; }
        /// <summary>
        /// The result
        /// </summary>
        public abstract CallResult? Result { get; set; }

        protected AsyncResetEvent _event;
        protected TimeSpan _timeout;
        protected CancellationTokenSource? _cts;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        protected BasePendingRequest(int id, object request, TimeSpan timeout)
        {
            Id = id;
            _event = new AsyncResetEvent(false, false);
            _timeout = timeout;
            Request = request;
            RequestTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Signal that the request has been send and the timeout timer should start
        /// </summary>
        public void IsSend()
        {
            // Start timeout countdown
            _cts = new CancellationTokenSource(_timeout);
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
        /// Process a response
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task ProcessAsync(DataEvent<BaseParsedMessage> message);
    }

    /// <summary>
    /// Pending socket request
    /// </summary>
    /// <typeparam name="T">The response data type</typeparam>
    public class PendingRequest<T> : BasePendingRequest
    {
        /// <inheritdoc />
        public override CallResult? Result { get; set;  }
        /// <summary>
        /// The typed call result
        /// </summary>
        public CallResult<T>? TypedResult => (CallResult<T>?)Result;
        /// <summary>
        /// Data handler
        /// </summary>
        public Func<DataEvent<ParsedMessage<T>>, Task<CallResult<T>>> Handler { get; }
        /// <summary>
        /// The response object type
        /// </summary>
        public override Type? ResponseType => typeof(T);

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="messageHandler"></param>
        /// <param name="timeout"></param>
        private PendingRequest(int id, object request, Func<DataEvent<ParsedMessage<T>>, Task<CallResult<T>>> messageHandler, TimeSpan timeout) 
            : base(id, request, timeout)
        {
            Handler = messageHandler;
        }

        /// <summary>
        /// Create a new pending request for provided query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static PendingRequest<T> CreateForQuery(Query<T> query, int id)
        {
            return new PendingRequest<T>(id, query.Request, async x =>
            {
                var response = await query.HandleMessageAsync(x).ConfigureAwait(false);
                return response.As(response.Data);
            }, TimeSpan.FromSeconds(5));
        }

        /// <inheritdoc />
        public override void Timeout()
        {
            Completed = true;
            Result = new CallResult<T>(new CancellationRequestedError());
        }

        /// <inheritdoc />
        public override void Fail(string error)
        {
            Result = new CallResult<T>(new ServerError(error));
            Completed = true;
            _event.Set();
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(DataEvent<BaseParsedMessage> message)
        {
            Completed = true;
            Result = await Handler(message.As((ParsedMessage<T>)message.Data)).ConfigureAwait(false);
            _event.Set();
        }
    }
}
