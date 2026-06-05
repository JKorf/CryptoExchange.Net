using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CryptoExchange.Net.Objects;

/// <summary>
/// HTTP call result
/// </summary>
public record HttpResult : IHttpResult
{
    /// <summary>
    /// Create a new success HTTP result
    /// </summary>
    public static HttpResult<T> Ok<T>(
        string exchange,
        HttpStatusCode code,
        Version version,
        HttpResponseHeaders responseHeaders,
        TimeSpan elapsed,
        long? contentLength,
        string? originalData,
        int requestId,
        string uri,
        string? content,
        HttpMethod method,
        HttpRequestHeaders requestHeaders,
        ResultDataSource source,
        T data) =>
        new HttpResult<T>(exchange, data, null)
        {
            ResponseStatusCode = code,
            HttpVersion = version,
            ResponseHeaders = responseHeaders,
            ResponseTime = elapsed,
            ResponseLength = contentLength,
            OriginalData = originalData,
            RequestId = requestId,
            RequestUrl = uri,
            RequestBody = content,
            RequestMethod = method,
            RequestHeaders = requestHeaders,
            DataSource = source,
        };

    /// <summary>
    /// Create a new success HTTP result
    /// </summary>
    public static HttpResult<T> Ok<T>(IHttpResult result, T data, PageRequest? pageRequest = null) =>
        new HttpResult<T>(result.Exchange, data, null)
        {
            ResponseStatusCode = result.ResponseStatusCode,
            HttpVersion = result.HttpVersion,
            ResponseHeaders = result.ResponseHeaders,
            ResponseTime = result.ResponseTime,
            ResponseLength = result.ResponseLength,
            OriginalData = result.OriginalData,
            RequestId = result.RequestId,
            RequestUrl = result.RequestUrl,
            RequestBody = result.RequestBody,
            RequestMethod = result.RequestMethod,
            RequestHeaders = result.RequestHeaders,
            DataSource = result.DataSource,
            Error = result.Error,
            Data = data,
            NextPageRequest = pageRequest
        };

    /// <summary>
    /// Create a new error HTTP result
    /// </summary>
    public static HttpResult<T> Fail<T>(string exchange, Error error) => new HttpResult<T>(exchange, default, error);

    /// <summary>
    /// Create a new error HTTP result
    /// </summary>
    public static HttpResult<T> Fail<T>(IHttpResult result, Error? error = null, T? data = default)
        => new HttpResult<T>(result.Exchange, data, error ?? result.Error)
        {
            ResponseStatusCode = result.ResponseStatusCode,
            HttpVersion = result.HttpVersion,
            ResponseHeaders = result.ResponseHeaders,
            ResponseTime = result.ResponseTime,
            ResponseLength = result.ResponseLength,
            OriginalData = result.OriginalData,
            RequestId = result.RequestId,
            RequestUrl = result.RequestUrl,
            RequestBody = result.RequestBody,
            RequestMethod = result.RequestMethod,
            RequestHeaders = result.RequestHeaders,
            DataSource = result.DataSource,
        };

    /// <summary>
    /// Create a new error HTTP result
    /// </summary>
    public static HttpResult<T> Fail<T>(
        string exchange,
        HttpStatusCode? code,
        Version? version,
        HttpResponseHeaders? responseHeaders,
        TimeSpan elapsed,
        long? contentLength,
        string? originalData,
        int requestId,
        string uri,
        string? content,
        HttpMethod method,
        HttpRequestHeaders requestHeaders,
        ResultDataSource source,
        Error error,
        T? result = default) =>
        new HttpResult<T>(exchange, result, error)
        {
            ResponseStatusCode = code,
            HttpVersion = version,
            ResponseHeaders = responseHeaders,
            ResponseTime = elapsed,
            ResponseLength = contentLength,
            OriginalData = originalData,
            RequestId = requestId,
            RequestUrl = uri,
            RequestBody = content,
            RequestMethod = method,
            RequestHeaders = requestHeaders,
            DataSource = source,
        };


    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; init; } = string.Empty;

    /// <inheritdoc />
    public Error? Error { get; internal set; }
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => Error == null;
    /// <summary>
    /// The original data returned by the call, only available when `OutputOriginalData` is set to `true` in the client options
    /// </summary>
    public string? OriginalData { get; init; }
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


/// <inheritdoc />
public record HttpResult<T> : HttpResult, IHttpResult<T>
{
    /// <summary>
    /// ctor
    /// </summary>
    public HttpResult(string exchange, T? value, Error? error)
    {
        Exchange = exchange;
        Data = value;
        Error = error;
    }

    /// <inheritdoc />
    public new Error? Error
    {
        get => base.Error;
        internal set => base.Error = value;
    }
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
    public new bool Success => Error == null;
    
    /// <summary>
    /// The data returned by the call, only available when Success = true
    /// </summary>
    public T? Data { get; init; }
    
    /// <summary>
    /// Next page request, only potentially available when using Shared API's
    /// </summary>
    public PageRequest? NextPageRequest { get; init; }
}