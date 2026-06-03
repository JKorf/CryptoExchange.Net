using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CryptoExchange.Net.Objects;

public readonly struct Unit
{
    public static readonly Unit Value = default;
}

public interface IOpResult
{
    /// <summary>
    /// An error if the call didn't succeed, will always be filled if Success = false
    /// </summary>
    Error? Error { get; }

    /// <summary>
    /// Whether the call was successful
    /// </summary>
    bool Success { get; }
}

public class OpResult : IOpResult
{
    /// <summary>
    /// Static success result
    /// </summary>
    public static OpResult SuccessResult { get; } = new OpResult();

    /// <inheritdoc />
    public Error? Error { get; init; }
    /// <inheritdoc />
#if NET5_0_OR_GREATER
        [MemberNotNullWhen(false, nameof(Error))]
#endif
    public bool Success => Error == null;

    /// <inheritdoc />
    public override string ToString()
    {
        return Success ? $"Success" : $"Error: {Error}";
    }
}

public class HttpResult<T> : IOpResult
{
    public HttpResult(T value, Error? error)
    {
        Data = value;
        Error = error;
    }

    /// <inheritdoc />
    public Error? Error { get; internal set; }
    /// <inheritdoc />
#if NET5_0_OR_GREATER
        [MemberNotNullWhen(false, nameof(Error))]
        [MemberNotNullWhen(true, nameof(Data))]
#endif
    public bool Success => Error == null;
    /// <summary>
    /// The original data returned by the call, only available when `OutputOriginalData` is set to `true` in the client options
    /// </summary>
    public string? OriginalData { get; init; }
    /// <summary>
    /// The data returned by the call, only available when Success = true
    /// </summary>
    public T? Data { get; init; }
    /// <summary>
    /// The request http method
    /// </summary>
    public HttpMethod? RequestMethod { get; init; }

    /// <summary>
    /// HTTP protocol version
    /// </summary>
    public Version? HttpVersion { get; init; }

    /// <summary>
    /// The headers sent with the request
    /// </summary>
    public HttpRequestHeaders? RequestHeaders { get; init; }

    /// <summary>
    /// The request id
    /// </summary>
    public int? RequestId { get; init; }

    /// <summary>
    /// The url which was requested
    /// </summary>
    public string? RequestUrl { get; init; }

    /// <summary>
    /// The body of the request
    /// </summary>
    public string? RequestBody { get; init; }

    /// <summary>
    /// Length in bytes of the response
    /// </summary>
    public long? ResponseLength { get; init; }

    /// <summary>
    /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
    /// </summary>
    public HttpStatusCode? ResponseStatusCode { get; init; }

    /// <summary>
    /// The response headers
    /// </summary>
    public HttpResponseHeaders? ResponseHeaders { get; init; }

    /// <summary>
    /// The time between sending the request and receiving the response
    /// </summary>
    public TimeSpan? ResponseTime { get; init; }
    /// <summary>
    /// The data source of this result
    /// </summary>
    public ResultDataSource DataSource { get; init; } = ResultDataSource.Server;
}

public class WebSocketResult<T> : IOpResult
{
    public WebSocketResult(T value, Error? error)
    {
        Data = value;
        Error = error;
    }

    /// <inheritdoc />
    public Error? Error { get; init; }
    /// <inheritdoc />
#if NET5_0_OR_GREATER
        [MemberNotNullWhen(false, nameof(Error))]
        [MemberNotNullWhen(true, nameof(Data))]
#endif
    public bool Success => Error == null;
    /// <summary>
    /// The original data returned by the call, only available when `OutputOriginalData` is set to `true` in the client options
    /// </summary>
    public string? OriginalData { get; init; }
    /// <summary>
    /// The data returned by the call, only available when Success = true
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// The request id
    /// </summary>
    public int? RequestId { get; init; }

    /// <summary>
    /// The url which was requested
    /// </summary>
    public int? ConnectionId { get; init; }

    /// <summary>
    /// The body of the request
    /// </summary>
    public string? RequestBody { get; init; }

    /// <summary>
    /// Length in bytes of the response
    /// </summary>
    public long? ResponseLength { get; init; }

    /// <summary>
    /// The time between sending the request and receiving the response
    /// </summary>
    public TimeSpan? ResponseTime { get; init; }
}