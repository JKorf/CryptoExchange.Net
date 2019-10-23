namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Base class for errors
    /// </summary>
    public abstract class Error
    {
        /// <summary>
        /// The error code
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// The message for the error that occured
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Optional data for the error
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected Error(int code, string message, object? data)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Code}: {Message} {Data}";
        }
    }

    /// <summary>
    /// Cant reach server error
    /// </summary>
    public class CantConnectError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CantConnectError() : base(1, "Can't connect to the server", null) { }
    }

    /// <summary>
    /// No api credentials provided while trying to access private endpoint
    /// </summary>
    public class NoApiCredentialsError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public NoApiCredentialsError() : base(2, "No credentials provided for private endpoint", null) { }
    }

    /// <summary>
    /// Error returned by the server
    /// </summary>
    public class ServerError: Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        public ServerError(string message, object? data = null) : base(3, "Server error: " + message, data) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        public ServerError(int code, string message, object? data = null) : base(code, message, data)
        {
        }
    }

    /// <summary>
    /// Web error returned by the server
    /// </summary>
    public class WebError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        public WebError(object? data) : base(4, "Web error", data) { }
    }

    /// <summary>
    /// Error while deserializing data
    /// </summary>
    public class DeserializeError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data">Deserializing data</param>
        public DeserializeError(object? data) : base(5, "Error deserializing data", data) { }
    }

    /// <summary>
    /// Unknown error
    /// </summary>
    public class UnknownError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data">Error data</param>
        public UnknownError(object? data = null) : base(6, "Unknown error occured", data) { }
    }

    /// <summary>
    /// An invalid parameter has been provided
    /// </summary>
    public class ArgumentError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        public ArgumentError(string message) : base(7, "Invalid parameter: " + message, null) { }
    }

    /// <summary>
    /// Rate limit exceeded
    /// </summary>
    public class RateLimitError: Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        public RateLimitError(string message) : base(8, "Rate limit exceeded: " + message, null) { }
    }

    /// <summary>
    /// Cancellation requested
    /// </summary>
    public class CancellationRequestedError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancellationRequestedError() : base(9, "Cancellation requested", null) { }
    }
}
