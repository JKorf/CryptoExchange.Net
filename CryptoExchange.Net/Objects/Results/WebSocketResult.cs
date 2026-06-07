using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.Objects;

/// <summary>
/// WebSocket call result
/// </summary>
public record WebSocketResult : IWebSocketResult
{
    /// <summary>
    /// ctor
    /// </summary>
    public WebSocketResult(string exchange, Error? error)
    {
        Exchange = exchange;
        Error = error;
    }

    /// <summary>
    /// Create a new success WebSocket result
    /// </summary>
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

    /// <summary>
    /// Create a new success WebSocket result
    /// </summary>
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

    /// <summary>
    /// Create a new success WebSocket result
    /// </summary>
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

    /// <summary>
    /// Create a new error WebSocket result
    /// </summary>
    public static WebSocketResult Fail(string exchange, Error error) => new WebSocketResult(exchange, error);
    /// <summary>
    /// Create a new error WebSocket result
    /// </summary>
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
    /// <summary>
    /// Create a new error WebSocket result
    /// </summary>
    public static WebSocketResult<T> Fail<T>(IWebSocketResult result, Error? error = null, T? data = default)
        => new WebSocketResult<T>(result.Exchange, data, error ?? result.Error)
        {
            ConnectionId = result.ConnectionId,
            Url = result.Url,
            RequestId = result.RequestId,
            ResponseTime = result.ResponseTime,
        };
    /// <summary>
    /// Create a new error WebSocket result
    /// </summary>
    public static WebSocketResult<T> Fail<T>(string exchange, Error error) => new WebSocketResult<T>(exchange, default, error);

    /// <summary>
    /// Create a new error WebSocket result
    /// </summary>
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


    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; init; }
    /// <inheritdoc />
    public Error? Error { get; init; }
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => Error == null;

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

/// <inheritdoc />
public record WebSocketResult<T> : WebSocketResult, IWebSocketResult<T>
{
    /// <summary>
    /// ctor
    /// </summary>
    public WebSocketResult(string exchange, T? value, Error? error): base(exchange, error)
    {
        Data = value;
    }

    /// <inheritdoc />
    public new Error? Error
    {
        get => base.Error;
        init => base.Error = value;
    }
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
    public new bool Success => Error == null;
    /// <summary>
    /// The data returned by the call, only available when Success = true
    /// </summary>
    public T? Data { get; init; }    
}

/// <inheritdoc />
public record QueryResult
{
    /// <summary>
    /// Create a new success query result
    /// </summary>
    public static QueryResult<T> Ok<T>(
        string exchange,
        int connectionId,
        TimeSpan elapsed,
        int requestId,
        string? requestBody,
        string? url,
        string? originalData,
        T data) =>
        new QueryResult<T>(exchange, data, null)
        {
            ResponseTime = elapsed,
            RequestId = requestId,
            RequestBody = requestBody,
            ConnectionId = connectionId,
            Url = url,
            OriginalData = originalData,
        };
    /// <summary>
    /// Create a new success WebSocket result
    /// </summary>
    public static QueryResult<T> Ok<T>(IQueryResult result, T data) =>
        new QueryResult<T>(result.Exchange, data, null)
        {
            ConnectionId = result.ConnectionId,
            Url = result.Url,
            RequestId = result.RequestId,
            RequestBody = result.RequestBody,
            ResponseTime = result.ResponseTime,
            Error = result.Error,
            OriginalData = result.OriginalData,
            Data = data
        };

    /// <summary>
    /// Create a new error WebSocket result
    /// </summary>
    public static QueryResult<T> Fail<T>(
        string exchange,
        int connectionId,
        TimeSpan elapsed,
        int requestId,
        string? requestBody,
        string? url,
        string? originalData,
        Error error) =>
        new QueryResult<T>(exchange, default, error)
        {
            ResponseTime = elapsed,
            RequestId = requestId,
            RequestBody = requestBody,
            ConnectionId = connectionId,
            OriginalData = originalData,
            Url = url
        };
    /// <summary>
    /// Create a new error WebSocket result
    /// </summary>
    public static QueryResult<T> Fail<T>(IQueryResult result, Error? error = null, T? data = default)
        => new QueryResult<T>(result.Exchange, data, error ?? result.Error)
        {
            ConnectionId = result.ConnectionId,
            Url = result.Url,
            RequestId = result.RequestId,
            RequestBody = result.RequestBody,
            OriginalData = result.OriginalData,
            ResponseTime = result.ResponseTime,
        };
    /// <summary>
    /// Create a new error WebSocket result
    /// </summary>
    public static QueryResult<T> Fail<T>(string exchange, Error error) => new QueryResult<T>(exchange, default, error);

}

/// <inheritdoc />
public record QueryResult<T> : WebSocketResult<T>, IQueryResult<T>
{
    /// <summary>
    /// ctor
    /// </summary>
    public QueryResult(string exchange, T? value, Error? error) : base(exchange, value, error)
    {
    }

    
    /// <inheritdoc />
    public new Error? Error
    {
        get => base.Error;
        init => base.Error = value;
    }
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
    public new bool Success => Error == null;
    /// <inheritdoc />
    public new T? Data
    {
        get => base.Data;
        init => base.Data = value;
    }

    /// <inheritdoc />
    public string? OriginalData { get; init; }
    /// <inheritdoc />
    public string? RequestBody { get; init; }
}