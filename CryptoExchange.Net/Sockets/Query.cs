using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sockets;

/// <summary>
/// Query 
/// </summary>
public abstract class Query : IMessageProcessor, IDisposable
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
    /// Timeout for the request
    /// </summary>
    public TimeSpan? RequestTimeout { get; set; }

    /// <summary>
    /// What should happen if the query times out
    /// </summary>
    public TimeoutBehavior TimeoutBehavior { get; set; } = TimeoutBehavior.Fail;

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
    /// Matcher for this query
    /// </summary>
    public MessageMatcher MessageMatcher { get; set; } = null!;

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
    /// Whether the query should wait for a response or not
    /// </summary>
    public bool ExpectsResponse { get; set; } = true;

    /// <summary>
    /// Wait event for response
    /// </summary>
    protected AsyncResetEvent _event;

    /// <summary>
    /// Cancellation token
    /// </summary>
    protected CancellationTokenSource? _cts;
    private bool _disposedValue;

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
        RequestTimestamp = DateTime.UtcNow;
        if (ExpectsResponse)
        {
            // Start timeout countdown
            _cts = new CancellationTokenSource(timeout);
            _cts.Token.Register(Timeout, false);
        }
        else
        {
            Completed = true;
            Result = CallResult.SuccessResult;
            _event.Set();
        }
    }

    /// <summary>
    /// Wait until timeout or the request is completed
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task WaitAsync(TimeSpan timeout, CancellationToken ct) => await _event.WaitAsync(timeout, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public virtual CallResult<object> Deserialize(IMessageAccessor accessor, Type type) => accessor.Deserialize(type);

    /// <summary>
    /// Mark request as timeout
    /// </summary>
    public abstract void Timeout();

    /// <summary>
    /// Mark request as failed
    /// </summary>
    public abstract void Fail(Error error);

    /// <summary>
    /// Handle a response message
    /// </summary>
    public abstract Task<CallResult> Handle(SocketConnection connection, DataEvent<object> message, MessageHandlerLink matchedHandler);

    /// <summary>
    /// Dispose
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _cts?.Dispose();
                _event.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Query
/// </summary>
/// <typeparam name="THandlerResponse">The type to be returned to the caller</typeparam>
public abstract class Query<THandlerResponse> : Query
{
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
    public override async Task<CallResult> Handle(SocketConnection connection, DataEvent<object> message, MessageHandlerLink matchedHandler)
    {
        if (!PreCheckMessage(connection, message))
            return CallResult.SuccessResult;

        CurrentResponses++;
        if (CurrentResponses == RequiredResponses)            
            Response = message.Data;

        if (Result?.Success != false)
            // If an error result is already set don't override that
            Result = matchedHandler.Handle(connection, message);

        if (CurrentResponses == RequiredResponses)
        {
            Completed = true;
            _event.Set();
            if (ContinueAwaiter != null)
                await ContinueAwaiter.WaitAsync().ConfigureAwait(false);
        }

        return Result;
    }

    /// <summary>
    /// Validate if a message is actually processable by this query
    /// </summary>
    public virtual bool PreCheckMessage(SocketConnection connection, DataEvent<object> message) => true;

    /// <inheritdoc />
    public override void Timeout()
    {
        if (Completed)
            return;
                    
        Completed = true;
        if (TimeoutBehavior == TimeoutBehavior.Fail)
            Result = new CallResult<THandlerResponse>(new TimeoutError());
        else
            Result = new CallResult<THandlerResponse>(default, null, default);

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
