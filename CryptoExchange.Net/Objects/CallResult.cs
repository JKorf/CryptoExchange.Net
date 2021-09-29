using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

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
        
        /// <summary>
        /// Create an error result
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static WebCallResult CreateErrorResult(Error error)
        {
            return new WebCallResult(null, null, error);
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
        public string? OriginalData { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="error"></param>
#pragma warning disable 8618
        public CallResult([AllowNull]T data, Error? error): base(error)
#pragma warning restore 8618
        {
#pragma warning disable 8601
            Data = data;
#pragma warning restore 8601
        }

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
        /// Create an error result
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public new static WebCallResult<T> CreateErrorResult(Error error)
        {
            return new WebCallResult<T>(null, null, default, error);
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public CallResult<K> As<K>([AllowNull] K data)
        {
            return new CallResult<K>(data, Error);
        }
    }

    /// <summary>
    /// The result of a request
    /// </summary>
    public class WebCallResult : CallResult
    {
        /// <summary>
        /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
        /// </summary>
        public HttpStatusCode? ResponseStatusCode { get; set; }

        /// <summary>
        /// The response headers
        /// </summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? ResponseHeaders { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code">Status code</param>
        /// <param name="responseHeaders">Response headers</param>
        /// <param name="error">Error</param>
        public WebCallResult(
            HttpStatusCode? code,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders, Error? error) : base(error)
        {
            ResponseHeaders = responseHeaders;
            ResponseStatusCode = code;
        }

        /// <summary>
        /// Create an error result
        /// </summary>
        /// <param name="code">Status code</param>
        /// <param name="responseHeaders">Response headers</param>
        /// <param name="error">Error</param>
        /// <returns></returns>
        public static WebCallResult CreateErrorResult(HttpStatusCode? code, IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders, Error error)
        {
            return new WebCallResult(code, responseHeaders, error);
        }

        /// <summary>
        /// Create an error result
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static WebCallResult CreateErrorResult(WebCallResult result)
        {
            return new WebCallResult(result.ResponseStatusCode, result.ResponseHeaders, result.Error);
        }
    }

    /// <summary>
    /// The result of a request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebCallResult<T>: CallResult<T>
    {
        /// <summary>
        /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
        /// </summary>
        public HttpStatusCode? ResponseStatusCode { get; set; }

        /// <summary>
        /// The response headers
        /// </summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>>? ResponseHeaders { get; set; }
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="data"></param>
        /// <param name="error"></param>
        public WebCallResult(
            HttpStatusCode? code, 
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders, 
            [AllowNull] T data, 
            Error? error): base(data, error)
        {
            ResponseStatusCode = code;
            ResponseHeaders = responseHeaders;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="originalData"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="data"></param>
        /// <param name="error"></param>
        public WebCallResult(
            HttpStatusCode? code,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders,
            string? originalData,
            [AllowNull] T data,
            Error? error) : base(data, error)
        {
            OriginalData = originalData;
            ResponseStatusCode = code;
            ResponseHeaders = responseHeaders;
        }

        /// <summary>
        /// Copy the WebCallResult to a new data type
        /// </summary>
        /// <typeparam name="K">The new type</typeparam>
        /// <param name="data">The data of the new type</param>
        /// <returns></returns>
        public new WebCallResult<K> As<K>([AllowNull] K data)
        {
            return new WebCallResult<K>(ResponseStatusCode, ResponseHeaders, OriginalData, data, Error);
        }

        /// <summary>
        /// Create an error result
        /// </summary>
        /// <param name="code"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static WebCallResult<T> CreateErrorResult(HttpStatusCode? code, IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders, Error error)
        {
            return new WebCallResult<T>(code, responseHeaders, default, error);
        }
    }
}
