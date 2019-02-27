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

        public WebCallResult(HttpStatusCode? code, T data, Error error): base(data, error)
        {
            ResponseStatusCode = code;
        }
    }
}
