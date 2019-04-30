using System;
using System.Collections.Generic;
using System.Net;

namespace CryptoExchange.Net.Objects
{
    public class CallResult<T>
    {
        /// <summary>
        /// The data returned by the call
        /// </summary>
        public T Data { get; internal set; }
        /// <summary>
        /// An error if the call didn't succeed
        /// </summary>
        public Error Error { get; internal set; }
        /// <summary>
        /// Whether the call was successful
        /// </summary>
        public bool Success => Error == null;

        public CallResult(T data, Error error)
        {
            Data = data;
            Error = error;
        }
    }

    public class WebCallResult<T>: CallResult<T>
    {
        /// <summary>
        /// The status code of the response. Note that a OK status does not always indicate success, check the Success parameter for this.
        /// </summary>
        public HttpStatusCode? ResponseStatusCode { get; set; }

        public IEnumerable<Tuple<string, string>> ResponseHeaders { get; set; }

        public WebCallResult(HttpStatusCode? code, IEnumerable<Tuple<string, string>> responseHeaders, T data, Error error): base(data, error)
        {
            ResponseHeaders = responseHeaders;
            ResponseStatusCode = code;
        }

        public static WebCallResult<T> CreateErrorResult(Error error)
        {
            return new WebCallResult<T>(null, null, default(T), error);
        }
        public static WebCallResult<T> CreateErrorResult(HttpStatusCode? code, IEnumerable<Tuple<string, string>> responseHeaders, Error error)
        {
            return new WebCallResult<T>(code, responseHeaders, default(T), error);
        }
    }
}
