using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

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
        protected CallResult([AllowNull]T data, string? originalData, Error? error): base(error)
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
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="error">The error to return</param>
        /// <returns></returns>
        public CallResult<K> AsError<K>(Error error)
        {
            return new CallResult<K>(default, OriginalData, error);
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
        /// <param name="requestUrl"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestMethod"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="error"></param>
        public WebCallResult(
            HttpStatusCode? code,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders,
            TimeSpan? responseTime,
            string? requestUrl,
            string? requestBody,
            HttpMethod? requestMethod,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders,
            Error? error) : base(error)
        {
            ResponseStatusCode = code;
            ResponseHeaders = responseHeaders;
            ResponseTime = responseTime;

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
            return new WebCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestUrl, RequestBody, RequestMethod, RequestHeaders, error);
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
        /// Create a new result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="responseTime"></param>
        /// <param name="originalData"></param>
        /// <param name="requestUrl"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestMethod"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="data"></param>
        /// <param name="error"></param>
        public WebCallResult(
            HttpStatusCode? code,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders,
            TimeSpan? responseTime,
            string? originalData,
            string? requestUrl,
            string? requestBody,
            HttpMethod? requestMethod,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders,
            [AllowNull] T data,
            Error? error) : base(data, originalData, error)
        {
            ResponseStatusCode = code;
            ResponseHeaders = responseHeaders;
            ResponseTime = responseTime;

            RequestUrl = requestUrl;
            RequestBody = requestBody;
            RequestHeaders = requestHeaders;
            RequestMethod = requestMethod;
        }

        /// <summary>
        /// Create a new error result
        /// </summary>
        /// <param name="error">The error</param>
        public WebCallResult(Error? error) : this(null, null, null, null, null, null, null, null, default, error) { }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public new WebCallResult<K> As<K>([AllowNull] K data)
        {
            return new WebCallResult<K>(ResponseStatusCode, ResponseHeaders, ResponseTime, OriginalData, RequestUrl, RequestBody, RequestMethod, RequestHeaders, data, Error);
        }

        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public WebCallResult AsDataless()
        {
            return new WebCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestUrl, RequestBody, RequestMethod, RequestHeaders, Error);
        }

        /// <summary>
        /// Copy as a dataless result
        /// </summary>
        /// <returns></returns>
        public WebCallResult AsDatalessError(Error error)
        {
            return new WebCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestUrl, RequestBody, RequestMethod, RequestHeaders, error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="error">The error returned</param>
        /// <returns></returns>
        public new WebCallResult<K> AsError<K>(Error error)
        {
            return new WebCallResult<K>(ResponseStatusCode, ResponseHeaders, ResponseTime, OriginalData, RequestUrl, RequestBody, RequestMethod, RequestHeaders, default, error);
        }
    }
}
