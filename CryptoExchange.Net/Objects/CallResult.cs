using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// The result of an operation
    /// </summary>
    public class CallResult
    {
        /// <summary>
        /// An error if the call didn't succeed, will always be filled if Success = false
        /// </summary>
        public Error? Error { get; internal set; }

        /// <summary>
        /// Whether the call was successful
        /// </summary>
        public bool Success => Error == null;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="error"></param>
        public CallResult(Error? error)
        {
            Error = error;
        }

        /// <summary>
        /// Overwrite bool check so we can use if(callResult) instead of if(callResult.Success)
        /// </summary>
        /// <param name="obj"></param>
        public static implicit operator bool(CallResult obj)
        {
            return obj?.Success == true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Success ? $"Success" : $"Error: {Error}";
        }
    }

    /// <summary>
    /// The result of an operation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CallResult<T>: CallResult
    {
        /// <summary>
        /// The data returned by the call, only available when Success = true
        /// </summary>
        public T Data { get; internal set; }

        /// <summary>
        /// The original data returned by the call, only available when `OutputOriginalData` is set to `true` in the client options
        /// </summary>
        public string? OriginalData { get; internal set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="originalData"></param>
        /// <param name="error"></param>
#pragma warning disable 8618
        public CallResult([AllowNull]T data, string? originalData, Error? error): base(error)
#pragma warning restore 8618
        {
            OriginalData = originalData;
#pragma warning disable 8601
            Data = data;
#pragma warning restore 8601
        }

        /// <summary>
        /// Create a new data result
        /// </summary>
        /// <param name="data">The data to return</param>
        public CallResult(T data) : this(data, null, null) { }

        /// <summary>
        /// Create a new error result
        /// </summary>
        /// <param name="error">The erro rto return</param>
        public CallResult(Error error) : this(default, null, error) { }

        /// <summary>
        /// Create a new error result
        /// </summary>
        /// <param name="error">The error to return</param>
        /// <param name="originalData">The original response data</param>
        public CallResult(Error error, string? originalData) : this(default, originalData, error) { }

        /// <summary>
        /// Overwrite bool check so we can use if(callResult) instead of if(callResult.Success)
        /// </summary>
        /// <param name="obj"></param>
        public static implicit operator bool(CallResult<T> obj)
        {
            return obj?.Success == true;
        }

        /// <summary>
        /// Whether the call was successful or not. Useful for nullability checking.
        /// </summary>
        /// <param name="data">The data returned by the call.</param>
        /// <param name="error"><see cref="Error"/> on failure.</param>
        /// <returns><c>true</c> when <see cref="CallResult{T}"/> succeeded, <c>false</c> otherwise.</returns>
        public bool GetResultOrError([MaybeNullWhen(false)] out T data, [NotNullWhen(false)] out Error? error)
        {
            if (Success)
            {
                data = Data!;
                error = null;

                return true;
            }
            else
            {
                data = default;
                error = Error!;

                return false;
            }
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public CallResult<K> As<K>([AllowNull] K data)
        {
            return new CallResult<K>(data, OriginalData, Error);
        }

        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public CallResult AsDataless()
        {
            return new CallResult(null);
        }

        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public CallResult AsDatalessError(Error error)
        {
            return new CallResult(error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="error">The error to return</param>
        /// <returns></returns>
        public CallResult<K> AsError<K>(Error error)
        {
            return new CallResult<K>(default, OriginalData, error);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Success ? $"Success" : $"Error: {Error}";
        }
    }

    /// <summary>
    /// The result of a request
    /// </summary>
    public class WebCallResult : CallResult
    {
        /// <summary>
        /// The request http method
        /// </summary>
        public HttpMethod? RequestMethod { get; set; }

        /// <summary>
        /// The headers sent with the request
        /// </summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? RequestHeaders { get; set; }

        /// <summary>
        /// The request id
        /// </summary>
        public int? RequestId { get; set; }

        /// <summary>
        /// The url which was requested
        /// </summary>
        public string? RequestUrl { get; set; }

        /// <summary>
        /// The body of the request
        /// </summary>
        public string? RequestBody { get; set; }

        /// <summary>
        /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
        /// </summary>
        public HttpStatusCode? ResponseStatusCode { get; set; }

        /// <summary>
        /// The response headers
        /// </summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? ResponseHeaders { get; set; }

        /// <summary>
        /// The time between sending the request and receiving the response
        /// </summary>
        public TimeSpan? ResponseTime { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="responseTime"></param>
        /// <param name="requestId"></param>
        /// <param name="requestUrl"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestMethod"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="error"></param>
        public WebCallResult(
            HttpStatusCode? code,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders,
            TimeSpan? responseTime,
            int? requestId,
            string? requestUrl,
            string? requestBody,
            HttpMethod? requestMethod,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders,
            Error? error) : base(error)
        {
            ResponseStatusCode = code;
            ResponseHeaders = responseHeaders;
            ResponseTime = responseTime;
            RequestId = requestId;

            RequestUrl = requestUrl;
            RequestBody = requestBody;
            RequestHeaders = requestHeaders;
            RequestMethod = requestMethod;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="error"></param>
        public WebCallResult(Error error): base(error) { }

        /// <summary>
        /// Return the result as an error result
        /// </summary>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public WebCallResult AsError(Error error)
        {
            return new WebCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public WebCallResult<K> As<K>([AllowNull] K data)
        {
            return new WebCallResult<K>(ResponseStatusCode, ResponseHeaders, ResponseTime, 0, null, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, ResultDataSource.Server, data, Error);
        }

        /// <summary>
        /// Copy the WebCallResult to an ExchangeWebResult of a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="exchange">The exchange</param>
        /// <param name="tradeMode">Trade mode the result applies to</param>
        /// <param name="data">The data</param>
        /// <returns></returns>
        public ExchangeWebResult<K> AsExchangeResult<K>(string exchange, TradingMode tradeMode, [AllowNull] K data)
        {
            return new ExchangeWebResult<K>(exchange, tradeMode, this.As<K>(data));
        }

        /// <summary>
        /// Copy the WebCallResult to an ExchangeWebResult of a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="exchange">The exchange</param>
        /// <param name="tradeModes">Trade modes the result applies to</param>
        /// <param name="data">The data</param>
        /// <returns></returns>
        public ExchangeWebResult<K> AsExchangeResult<K>(string exchange, TradingMode[]? tradeModes, [AllowNull] K data)
        {
            return new ExchangeWebResult<K>(exchange, tradeModes, this.As<K>(data));
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public WebCallResult<K> AsError<K>(Error error)
        {
            return new WebCallResult<K>(ResponseStatusCode, ResponseHeaders, ResponseTime, 0, null, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, ResultDataSource.Server, default, error);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return (Success ? $"Success" : $"Error: {Error}") + $" in {ResponseTime}";
        }
    }

    /// <summary>
    /// The result of a request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebCallResult<T>: CallResult<T>
    {
        /// <summary>
        /// The request http method
        /// </summary>
        public HttpMethod? RequestMethod { get; set; }

        /// <summary>
        /// The headers sent with the request
        /// </summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? RequestHeaders { get; set; }

        /// <summary>
        /// The request id
        /// </summary>
        public int? RequestId { get; set; }

        /// <summary>
        /// The url which was requested
        /// </summary>
        public string? RequestUrl { get; set; }

        /// <summary>
        /// The body of the request
        /// </summary>
        public string? RequestBody { get; set; }

        /// <summary>
        /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
        /// </summary>
        public HttpStatusCode? ResponseStatusCode { get; set; }

        /// <summary>
        /// Length in bytes of the response
        /// </summary>
        public long? ResponseLength { get; set; }

        /// <summary>
        /// The response headers
        /// </summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? ResponseHeaders { get; set; }

        /// <summary>
        /// The time between sending the request and receiving the response
        /// </summary>
        public TimeSpan? ResponseTime { get; set; }

        /// <summary>
        /// The data source of this result
        /// </summary>
        public ResultDataSource DataSource { get; set; } = ResultDataSource.Server;

        /// <summary>
        /// Create a new result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="responseTime"></param>
        /// <param name="responseLength"></param>
        /// <param name="originalData"></param>
        /// <param name="requestId"></param>
        /// <param name="requestUrl"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestMethod"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="dataSource"></param>
        /// <param name="data"></param>
        /// <param name="error"></param>
        public WebCallResult(
            HttpStatusCode? code,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders,
            TimeSpan? responseTime,
            long? responseLength,
            string? originalData,
            int? requestId,
            string? requestUrl,
            string? requestBody,
            HttpMethod? requestMethod,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders,
            ResultDataSource dataSource,
            [AllowNull] T data,
            Error? error) : base(data, originalData, error)
        {
            ResponseStatusCode = code;
            ResponseHeaders = responseHeaders;
            ResponseTime = responseTime;
            ResponseLength = responseLength;

            RequestId = requestId;
            RequestUrl = requestUrl;
            RequestBody = requestBody;
            RequestHeaders = requestHeaders;
            RequestMethod = requestMethod;
            DataSource = dataSource;
        }

        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public new WebCallResult AsDataless()
        {
            return new WebCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, Error);
        }
        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public new WebCallResult AsDatalessError(Error error)
        {
            return new WebCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, error);
        }

        /// <summary>
        /// Create a new error result
        /// </summary>
        /// <param name="error">The error</param>
        public WebCallResult(Error? error) : this(null, null, null, null, null, null, null, null, null, null, ResultDataSource.Server, default, error) { }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public new WebCallResult<K> As<K>([AllowNull] K data)
        {
            return new WebCallResult<K>(ResponseStatusCode, ResponseHeaders, ResponseTime, ResponseLength, OriginalData, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, DataSource, data, Error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public new WebCallResult<K> AsError<K>(Error error)
        {
            return new WebCallResult<K>(ResponseStatusCode, ResponseHeaders, ResponseTime, ResponseLength, OriginalData, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, DataSource, default, error);
        }

        /// <summary>
        /// Copy the WebCallResult to an ExchangeWebResult of a new data type
        /// </summary>
        /// <param name="exchange">The exchange</param>
        /// <param name="tradeMode">Trade mode the result applies to</param>
        /// <returns></returns>
        public ExchangeWebResult<T> AsExchangeResult(string exchange, TradingMode tradeMode)
        {
            return new ExchangeWebResult<T>(exchange, tradeMode, this);
        }

        /// <summary>
        /// Copy the WebCallResult to an ExchangeWebResult of a new data type
        /// </summary>
        /// <param name="exchange">The exchange</param>
        /// <param name="tradeModes">Trade modes the result applies to</param>
        /// <returns></returns>
        public ExchangeWebResult<T> AsExchangeResult(string exchange, TradingMode[] tradeModes)
        {
            return new ExchangeWebResult<T>(exchange, tradeModes, this);
        }

        /// <summary>
        /// Copy the WebCallResult to an ExchangeWebResult of a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="exchange">The exchange</param>
        /// <param name="tradeMode">Trade mode the result applies to</param>
        /// <param name="data">Data</param>
        /// <param name="nextPageToken">Next page token</param>
        /// <returns></returns>
        public ExchangeWebResult<K> AsExchangeResult<K>(string exchange, TradingMode tradeMode, [AllowNull] K data, INextPageToken? nextPageToken = null)
        {
            return new ExchangeWebResult<K>(exchange, tradeMode, As<K>(data), nextPageToken);
        }

        /// <summary>
        /// Copy the WebCallResult to an ExchangeWebResult of a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="exchange">The exchange</param>
        /// <param name="tradeModes">Trade modes the result applies to</param>
        /// <param name="data">Data</param>
        /// <param name="nextPageToken">Next page token</param>
        /// <returns></returns>
        public ExchangeWebResult<K> AsExchangeResult<K>(string exchange, TradingMode[]? tradeModes, [AllowNull] K data, INextPageToken? nextPageToken = null)
        {
            return new ExchangeWebResult<K>(exchange, tradeModes, As<K>(data), nextPageToken);
        }

        /// <summary>
        /// Copy the WebCallResult to an ExchangeWebResult with a specific error
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="exchange">The exchange</param>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public ExchangeWebResult<K> AsExchangeError<K>(string exchange, Error error)
        {
            return new ExchangeWebResult<K>(exchange, null, AsError<K>(error));
        }

        /// <summary>
        /// Return a copy of this result with data source set to cache
        /// </summary>
        /// <returns></returns>
        internal WebCallResult<T> Cached()
        {
            return new WebCallResult<T>(ResponseStatusCode, ResponseHeaders, ResponseTime, ResponseLength, OriginalData, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, ResultDataSource.Cache, Data, Error);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Success ? $"Success response" : $"Error response: {Error}");
            if (ResponseLength != null)
                sb.Append($", {ResponseLength} bytes");
            if (ResponseTime != null)
                sb.Append($" received in {Math.Round(ResponseTime?.TotalMilliseconds ?? 0)}ms");

            return sb.ToString();
        }
    }
}
