using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.Objects;

public abstract record BaseWebSocketResult: IWebSocketResult
{
    public string Exchange { get; init; }

#if NET5_0_OR_GREATER
    [MemberNotNullWhen(false, nameof(Error))]
#endif
    public abstract bool Success { get; }
    public abstract Error? Error { get; init; }

    /// <summary>
    /// The request id
    /// </summary>
    public int? RequestId { get; init; }

    /// <summary>
    /// The url which was requested
    /// </summary>
    public int? ConnectionId { get; init; }

    /// <summary>
    /// The websocket url
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// The time between sending the request and receiving the response
    /// </summary>
    public TimeSpan? ResponseTime { get; init; }

}

public record WebSocketResult : BaseWebSocketResult
{
    public WebSocketResult(string exchange, Error? error)
    {
        Exchange = exchange;
        Error = error;
    }

    public static WebSocketResult Fail(string exchange, Error error) => new WebSocketResult(exchange, error);
    public static WebSocketResult Ok(
        string exchange,
        int connectionId,
        TimeSpan elapsed,
        int requestId,
        string? url) =>
        new WebSocketResult(exchange, null)
        {
            ResponseTime = elapsed,
            RequestId = requestId,
            ConnectionId = connectionId,
            Url = url
        };

    public static WebSocketResult Fail(
        string exchange,
        int connectionId,
        TimeSpan elapsed,
        int requestId,
        string? url,
        Error error) =>
        new WebSocketResult(exchange, error)
        {
            ResponseTime = elapsed,
            RequestId = requestId,
            ConnectionId = connectionId,
            Url = url
        };


    public static WebSocketResult<T> Ok<T>(IWebSocketResult result, T data) =>
        new WebSocketResult<T>(result.Exchange, data, null)
        {
            ConnectionId = result.ConnectionId,
            Url = result.Url,
            RequestId = result.RequestId,
            ResponseTime = result.ResponseTime,
            Error = result.Error,
            Data = data
        };

    public static WebSocketResult<T> Fail<T>(IWebSocketResult result, Error? error = null)
        => new WebSocketResult<T>(result.Exchange, default, error ?? result.Error)
        {
            ConnectionId = result.ConnectionId,
            Url = result.Url,
            RequestId = result.RequestId,
            ResponseTime = result.ResponseTime,
        };
    public static WebSocketResult<T> Fail<T>(string exchange, Error error) => new WebSocketResult<T>(exchange, default, error);
    public static WebSocketResult<T> Ok<T>(
        string exchange,
        int connectionId,
        TimeSpan elapsed,
        int requestId,
        string? url,
        T data) =>
        new WebSocketResult<T>(exchange, data, null)
        {
            ResponseTime = elapsed,
            RequestId = requestId,
            ConnectionId = connectionId,
            Url = url
        };

    public static WebSocketResult<T> Fail<T>(
        string exchange,
        int connectionId,
        TimeSpan elapsed,
        int requestId,
        string? url,
        Error error) =>
        new WebSocketResult<T>(exchange, default, error)
        {
            ResponseTime = elapsed,
            RequestId = requestId,
            ConnectionId = connectionId,
            Url = url
        };

    /// <inheritdoc />
    public override Error? Error { get; init; }
    /// <inheritdoc />
#if NET5_0_OR_GREATER
    [MemberNotNullWhen(false, nameof(Error))]
#endif
    public override bool Success => Error == null;
}

public record WebSocketResult<T> : BaseWebSocketResult
{
    public WebSocketResult(string exchange, T? value, Error? error)
    {
        Exchange = exchange;
        Data = value;
        Error = error;
    }


    /// <inheritdoc />
    public override Error? Error { get; init; }
    /// <inheritdoc />
#if NET5_0_OR_GREATER
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
#endif
    public override bool Success => Error == null;
    /// <summary>
    /// The data returned by the call, only available when Success = true
    /// </summary>
    public T? Data { get; init; }

    
}