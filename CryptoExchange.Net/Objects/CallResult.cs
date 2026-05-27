using CryptoExchange.Net.SharedApis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Call result
    /// </summary>
    public interface ICallResult
    {
        /// <summary>
        /// An error if the call didn't succeed, will always be filled if Success = false
        /// </summary>
        Error? Error { get; }
        /// <summary>
        /// Whether the call was successful
        /// </summary>
        bool Success { get; }
        /// <summary>
        /// The original data returned by the call, only available when `OutputOriginalData` is set to `true` in the client options
        /// </summary>
        public string? OriginalData { get; }
    }

    public interface ICallResult<T> : ICallResult
    {
        /// <summary>
        /// The data returned by the call, only available when Success = true
        /// </summary>
        T? Data { get; }
    }

    /// <summary>
    /// Web call result
    /// </summary>
    public interface IWebCallResult : ICallResult
    {
        /// <summary>
        /// The request http method
        /// </summary>
        HttpMethod? RequestMethod { get; }
        /// <summary>
        /// HTTP protocol version
        /// </summary>
        Version? HttpVersion { get; }
        /// <summary>
        /// The headers sent with the request
        /// </summary>
        HttpRequestHeaders? RequestHeaders { get; }
        /// <summary>
        /// The request id
        /// </summary>
        int? RequestId { get; }
        /// <summary>
        /// The url which was requested
        /// </summary>
        string? RequestUrl { get; }
        /// <summary>
        /// The body of the request
        /// </summary>
        string? RequestBody { get; }
        /// <summary>
        /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
        /// </summary>
        HttpStatusCode? ResponseStatusCode { get; }
        /// <summary>
        /// Length in bytes of the response
        /// </summary>
        long? ResponseLength { get; }
        /// <summary>
        /// The response headers
        /// </summary>
        HttpResponseHeaders? ResponseHeaders { get; }
        /// <summary>
        /// The time between sending the request and receiving the response
        /// </summary>
        TimeSpan? ResponseTime { get; }
        /// <summary>
        /// The data source of this result
        /// </summary>
        ResultDataSource DataSource { get; }
    }

    public interface IWebCallResult<T> : IWebCallResult, ICallResult<T>
    {
    }

    /// <summary>
    /// The result of an operation
    /// </summary>
    public record CallResult : ICallResult
    {
        /// <summary>
        /// Static success result
        /// </summary>
        public static CallResult SuccessResult { get; } = new CallResult((Error?)null);

        /// <summary>
        /// An error if the call didn't succeed, will always be filled if Success = false
        /// </summary>
        public Error? Error { get; internal set; }

        /// <summary>
        /// The original data returned by the call, only available when `OutputOriginalData` is set to `true` in the client options
        /// </summary>
        public string? OriginalData { get; internal set; }

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
    public record CallResult<T>: CallResult, ICallResult<T>
    {
        /// <summary>
        /// The data returned by the call, only available when Success = true
        /// </summary>
        public T? Data { get; internal set; }

        /// <summary>
        /// ctor
        /// </summary>
        public CallResult(T? data, string? originalData, Error? error): base(error)
        {
            OriginalData = originalData;
            Data = data;
        }

        /// <summary>
        /// Create a new data result
        /// </summary>
        /// <param name="data">The data to return</param>
        public CallResult(T? data) : this(data, null, null) { }

        /// <summary>
        /// Create a new error result
        /// </summary>
        /// <param name="error">The error to return</param>
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
        /// Copy the WebCallResult to a new data type with default value
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <returns></returns>
        public CallResult<K> As<K>()
        {
            return new CallResult<K>(default, OriginalData, Error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public CallResult<K> As<K>(K? data)
        {
            return new CallResult<K>(data, OriginalData, Error);
        }

        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public CallResult AsDataless()
        {
            if (Error != null )
                return new CallResult(Error);

            return SuccessResult;
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
        /// Copy the CallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data</param>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public CallResult<K> AsErrorWithData<K>(Error error, K data)
        {
            return new CallResult<K>(data, OriginalData, error);
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
    public record WebCallResult<T> : CallResult<T>, IWebCallResult
    {
        /// <summary>
        /// The request http method
        /// </summary>
        public HttpMethod? RequestMethod { get; set; }
        
        /// <summary>
        /// HTTP protocol version
        /// </summary>
        public Version? HttpVersion { get; set; }

        /// <summary>
        /// The headers sent with the request
        /// </summary>
        public HttpRequestHeaders? RequestHeaders { get; set; }

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
        /// Length in bytes of the response
        /// </summary>
        public long? ResponseLength { get; set; }

        /// <summary>
        /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
        /// </summary>
        public HttpStatusCode? ResponseStatusCode { get; set; }

        /// <summary>
        /// The response headers
        /// </summary>
        public HttpResponseHeaders? ResponseHeaders { get; set; }

        /// <summary>
        /// The time between sending the request and receiving the response
        /// </summary>
        public TimeSpan? ResponseTime { get; set; }
        /// <summary>
        /// The data source of this result
        /// </summary>
        public ResultDataSource DataSource { get; set; } = ResultDataSource.Server;

        /// <summary>
        /// ctor
        /// </summary>
        public WebCallResult(
            HttpStatusCode? code,
            Version? httpVersion,
            HttpResponseHeaders? responseHeaders,
            TimeSpan? responseTime,
            long? responseLength,
            string? originalData,
            int? requestId,
            string? requestUrl,
            string? requestBody,
            HttpMethod? requestMethod,
            HttpRequestHeaders? requestHeaders,
            ResultDataSource dataSource,
            T? data,
            Error? error) : base(default, originalData, error)
        {
            ResponseStatusCode = code;
            HttpVersion = httpVersion;
            ResponseHeaders = responseHeaders;
            ResponseTime = responseTime;
            ResponseLength = responseLength;
            RequestId = requestId;

            RequestUrl = requestUrl;
            RequestBody = requestBody;
            RequestHeaders = requestHeaders;
            RequestMethod = requestMethod;

            DataSource = dataSource;
            Data = data;
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
        public WebCallResult<T> AsError(Error error)
        {
            return this with { Error = error };
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type with default value
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <returns></returns>
        public new WebCallResult<K> As<K>()
        {
            return new WebCallResult<K>(
                ResponseStatusCode, 
                HttpVersion, 
                ResponseHeaders, 
                ResponseTime,
                ResponseLength,
                OriginalData,
                RequestId, 
                RequestUrl, 
                RequestBody, 
                RequestMethod, 
                RequestHeaders, 
                DataSource,
                default,
                Error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public new WebCallResult<K> As<K>([AllowNull] K data)
        {
            return new WebCallResult<K>(
                ResponseStatusCode,
                HttpVersion, 
                ResponseHeaders, 
                ResponseTime,
                ResponseLength, 
                OriginalData,
                RequestId,
                RequestUrl, 
                RequestBody, 
                RequestMethod, 
                RequestHeaders,
                DataSource,
                data, 
                Error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public new WebCallResult<K> AsError<K>(Error error)
        {
            return new WebCallResult<K>(
                ResponseStatusCode,
                HttpVersion,
                ResponseHeaders,
                ResponseTime,
                ResponseLength,
                OriginalData,
                RequestId,
                RequestUrl,
                RequestBody,
                RequestMethod,
                RequestHeaders,
                DataSource,
                default,
                error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data</param>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public new WebCallResult<K> AsErrorWithData<K>(Error error, K data)
        {
            return new WebCallResult<K>(
                ResponseStatusCode,
                HttpVersion,
                ResponseHeaders,
                ResponseTime,
                ResponseLength,
                OriginalData,
                RequestId,
                RequestUrl,
                RequestBody,
                RequestMethod,
                RequestHeaders,
                DataSource,
                data,
                error);
        }

        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public new WebCallResult AsDataless()
        {
            return new WebCallResult(
                ResponseStatusCode, 
                HttpVersion,
                ResponseHeaders,
                ResponseTime,
                ResponseLength,
                OriginalData, 
                RequestId,
                RequestUrl,
                RequestBody,
                RequestMethod,
                RequestHeaders,
                DataSource,
                Error);
        }

        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public new WebCallResult AsDatalessError(Error error)
        {
            return new WebCallResult(
                ResponseStatusCode,
                HttpVersion, 
                ResponseHeaders, 
                ResponseTime,
                ResponseLength,
                OriginalData, 
                RequestId, 
                RequestUrl,
                RequestBody,
                RequestMethod,
                RequestHeaders,
                DataSource,
                error);
        }

        /// <summary>
        /// Return a copy of this result with data source set to cache
        /// </summary>
        /// <returns></returns>
        internal WebCallResult<T> Cached()
        {
            return new WebCallResult<T>(ResponseStatusCode, HttpVersion, ResponseHeaders, ResponseTime, ResponseLength, OriginalData, RequestId, RequestUrl, RequestBody, RequestMethod, RequestHeaders, ResultDataSource.Cache, Data, Error);
        }

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult of a new data type
        ///// </summary>
        ///// <typeparam name="K">The new type</typeparam>
        ///// <param name="exchange">The exchange</param>
        ///// <param name="tradeMode">Trade mode the result applies to</param>
        ///// <param name="data">The data</param>
        ///// <returns></returns>
        //public ExchangeWebResult<K> AsExchangeResult<K>(string exchange, TradingMode tradeMode, [AllowNull] K data)
        //{
        //    return new ExchangeWebResult<K>(exchange, tradeMode, this.As<K>(data));
        //}

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult of a new data type
        ///// </summary>
        ///// <typeparam name="K">The new type</typeparam>
        ///// <param name="exchange">The exchange</param>
        ///// <param name="tradeModes">Trade modes the result applies to</param>
        ///// <param name="data">The data</param>
        ///// <returns></returns>
        //public ExchangeWebResult<K> AsExchangeResult<K>(string exchange, TradingMode[]? tradeModes, [AllowNull] K data)
        //{
        //    return new ExchangeWebResult<K>(exchange, tradeModes, this.As<K>(data));
        //}

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult of a new data type
        ///// </summary>
        ///// <typeparam name="K">The new type</typeparam>
        ///// <param name="exchange">The exchange</param>
        ///// <returns></returns>
        //public ExchangeWebResult<K> AsExchangeResult<K>(string exchange)
        //{
        //    return new ExchangeWebResult<K>(exchange, (TradingMode)default, this.As<K>(default));
        //}

        ///// <summary>
        ///// Create a new exchange result with a specific error
        ///// </summary>
        ///// <typeparam name="K">The new type</typeparam>
        ///// <param name="exchange">The exchange</param>
        ///// <param name="error">The error</param>
        ///// <returns></returns>
        //public ExchangeWebResult<K> AsExchangeError<K>(string exchange, Error error)
        //{
        //    return new ExchangeWebResult<K>(exchange, error);
        //}

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Success ? $"Success response" : $"Error response: {Error}");
            if (ResponseLength != null)
                sb.Append($", {ResponseLength} bytes");
            if (ResponseTime != null)
                sb.Append($", received in {Math.Round(ResponseTime?.TotalMilliseconds ?? 0)}ms");

            return sb.ToString();
        }
    }

    /// <summary>
    /// The result of a request
    /// </summary>
    public record WebCallResult : WebCallResult<object>, IWebCallResult<object>
    {
        /// <summary>
        /// Create a new result
        /// </summary>
        public WebCallResult(
            HttpStatusCode? code,
            Version? httpVersion,
            HttpResponseHeaders? responseHeaders,
            TimeSpan? responseTime,
            long? responseLength,
            string? originalData,
            int? requestId,
            string? requestUrl,
            string? requestBody,
            HttpMethod? requestMethod,
            HttpRequestHeaders? requestHeaders,
            ResultDataSource dataSource,
            Error? error) : base(
                code,
                httpVersion,
                responseHeaders,
                responseTime,
                responseLength,
                originalData,
                requestId,
                requestUrl,
                requestBody,
                requestMethod,
                requestHeaders,
                dataSource,
                default,
                error)
        {
        }

        /// <summary>
        /// Create a new error result
        /// </summary>
        /// <param name="error">The error</param>
        public WebCallResult(Error? error) : this(null, null, null, null, null, null, null, null, null, null, null, ResultDataSource.Server, error) { }
                
        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public new WebCallResult<K> As<K>([AllowNull] K data)
        {
            return new WebCallResult<K>(
                ResponseStatusCode,
                HttpVersion,
                ResponseHeaders,
                ResponseTime,
                ResponseLength,
                OriginalData,
                RequestId,
                RequestUrl,
                RequestBody, 
                RequestMethod,
                RequestHeaders,
                DataSource,
                data,
                Error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public new WebCallResult<K> AsError<K>(Error error)
        {
            return new WebCallResult<K>(
                ResponseStatusCode, 
                HttpVersion, 
                ResponseHeaders, 
                ResponseTime,
                ResponseLength,
                OriginalData,
                RequestId,
                RequestUrl,
                RequestBody,
                RequestMethod,
                RequestHeaders,
                DataSource,
                default,
                error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data</param>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public new WebCallResult<K> AsErrorWithData<K>(Error error, K data)
        {
            return new WebCallResult<K>(
                ResponseStatusCode,
                HttpVersion,
                ResponseHeaders,
                ResponseTime, 
                ResponseLength,
                OriginalData,
                RequestId, 
                RequestUrl,
                RequestBody, 
                RequestMethod, 
                RequestHeaders,
                DataSource, 
                data,
                error);
        }

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult of a new data type
        ///// </summary>
        ///// <param name="exchange">The exchange</param>
        ///// <param name="tradeMode">Trade mode the result applies to</param>
        ///// <returns></returns>
        //public ExchangeWebResult<T> AsExchangeResult(string exchange, TradingMode tradeMode)
        //{
        //    return new ExchangeWebResult<T>(exchange, tradeMode, this);
        //}

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult of a new data type
        ///// </summary>
        ///// <param name="exchange">The exchange</param>
        ///// <param name="tradeModes">Trade modes the result applies to</param>
        ///// <returns></returns>
        //public ExchangeWebResult<T> AsExchangeResult(string exchange, TradingMode[] tradeModes)
        //{
        //    return new ExchangeWebResult<T>(exchange, tradeModes, this);
        //}

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult of a new data type
        ///// </summary>
        ///// <typeparam name="K">The new type</typeparam>
        ///// <param name="exchange">The exchange</param>
        ///// <returns></returns>
        //public ExchangeWebResult<K> AsExchangeResult<K>(string exchange)
        //{
        //    return new ExchangeWebResult<K>(exchange, (TradingMode)default, this.As<K>(default));
        //}

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult of a new data type
        ///// </summary>
        ///// <typeparam name="K">The new type</typeparam>
        ///// <param name="exchange">The exchange</param>
        ///// <param name="tradeMode">Trade mode the result applies to</param>
        ///// <param name="data">Data</param>
        ///// <param name="nextPageRequest">Next page request</param>
        ///// <returns></returns>
        //public ExchangeWebResult<K> AsExchangeResult<K>(string exchange, TradingMode tradeMode, [AllowNull] K data, PageRequest? nextPageRequest = null)
        //{
        //    return new ExchangeWebResult<K>(exchange, tradeMode, As<K>(data), nextPageRequest);
        //}

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult of a new data type
        ///// </summary>
        ///// <typeparam name="K">The new type</typeparam>
        ///// <param name="exchange">The exchange</param>
        ///// <param name="tradeModes">Trade modes the result applies to</param>
        ///// <param name="data">Data</param>
        ///// <param name="nextPageRequest">Next page token</param>
        ///// <returns></returns>
        //public ExchangeWebResult<K> AsExchangeResult<K>(string exchange, TradingMode[]? tradeModes, [AllowNull] K data, PageRequest? nextPageRequest = null)
        //{
        //    return new ExchangeWebResult<K>(exchange, tradeModes, As<K>(data), nextPageRequest);
        //}

        ///// <summary>
        ///// Copy the WebCallResult to an ExchangeWebResult with a specific error
        ///// </summary>
        ///// <typeparam name="K">The new type</typeparam>
        ///// <param name="exchange">The exchange</param>
        ///// <param name="error">The error returned</param>
        ///// <returns></returns>
        //public ExchangeWebResult<K> AsExchangeError<K>(string exchange, Error error)
        //{
        //    return new ExchangeWebResult<K>(exchange, null, AsError<K>(error));
        //}
    }
}
