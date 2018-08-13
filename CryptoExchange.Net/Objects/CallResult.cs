namespace CryptoExchange.Net
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
}
