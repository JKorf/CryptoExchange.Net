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
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        protected Error(int code, string message)
        {
            Code = code;
            Message = message;
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Code}: {Message}";
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
        public CantConnectError() : base(1, "Can't connect to the server") { }
    }

    /// <summary>
    /// No api credentials provided while trying to access private endpoint
    /// </summary>
    public class NoApiCredentialsError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public NoApiCredentialsError() : base(2, "No credentials provided for private endpoint") { }
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
        public ServerError(string message) : base(3, "Server error: " + message) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ServerError(int code, string message) : base(code, message)
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
        /// <param name="message"></param>
        public WebError(string message) : base(4, "Web error: " + message) { }
    }

    /// <summary>
    /// Error while deserializing data
    /// </summary>
    public class DeserializeError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        public DeserializeError(string message) : base(5, "Error deserializing data: " + message) { }
    }

    /// <summary>
    /// Unknown error
    /// </summary>
    public class UnknownError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        public UnknownError(string message) : base(6, "Unknown error occured " + message) { }
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
        public ArgumentError(string message) : base(7, "Invalid parameter: " + message) { }
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
        public RateLimitError(string message) : base(8, "Rate limit exceeded: " + message) { }
    }

    /// <summary>
    /// Cancellation requested
    /// </summary>
    public class CancellationRequestedError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancellationRequestedError() : base(9, "Cancellation requested") { }
    }
}
