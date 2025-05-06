using System;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Base class for errors
    /// </summary>
    public abstract class Error
    {
        /// <summary>
        /// The error code from the server
        /// </summary>
        public int? Code { get; set; }

        /// <summary>
        /// The message for the error that occurred
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Underlying exception
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        protected Error (int? code, string message, Exception? exception)
        {
            Code = code;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Code != null ? $"[{GetType().Name}] {Code}: {Message}" : $"[{GetType().Name}] {Message}";
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
        public CantConnectError() : base(null, "Can't connect to the server", null) { }

        /// <summary>
        /// ctor
        /// </summary>
        public CantConnectError(Exception? exception) : base(null, "Can't connect to the server", exception) { }

        /// <summary>
        /// ctor
        /// </summary>
        protected CantConnectError(int? code, string message, Exception? exception) : base(code, message, exception) { }
    }

    /// <summary>
    /// No api credentials provided while trying to access a private endpoint
    /// </summary>
    public class NoApiCredentialsError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public NoApiCredentialsError() : base(null, "No credentials provided for private endpoint", null) { }

        /// <summary>
        /// ctor
        /// </summary>
        protected NoApiCredentialsError(int? code, string message, Exception? exception) : base(code, message, exception) { }
    }

    /// <summary>
    /// Error returned by the server
    /// </summary>
    public class ServerError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ServerError(string message) : base(null, message, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        public ServerError(int? code, string message, Exception? exception = null) : base(code, message, exception) { }
    }

    /// <summary>
    /// Web error returned by the server
    /// </summary>
    public class WebError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public WebError(string message, Exception? exception = null) : base(null, message, exception) { }

        /// <summary>
        /// ctor
        /// </summary>
        public WebError(int code, string message, Exception? exception = null) : base(code, message, exception) { }
    }

    /// <summary>
    /// Error while deserializing data
    /// </summary>
    public class DeserializeError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DeserializeError(string message, Exception? exception = null) : base(null, message, exception) { }

        /// <summary>
        /// ctor
        /// </summary>
        protected DeserializeError(int? code, string message, Exception? exception = null) : base(code, message, exception) { }
    }

    /// <summary>
    /// Unknown error
    /// </summary>
    public class UnknownError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public UnknownError(string message, Exception? exception = null) : base(null, message, exception) { }

        /// <summary>
        /// ctor
        /// </summary>
        protected UnknownError(int? code, string message, Exception? exception = null): base(code, message, exception) { }
    }

    /// <summary>
    /// An invalid parameter has been provided
    /// </summary>
    public class ArgumentError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ArgumentError(string message) : base(null, "Invalid parameter: " + message, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        protected ArgumentError(int? code, string message, Exception? exception = null) : base(code, message, exception) { }
    }

    /// <summary>
    /// Rate limit exceeded (client side)
    /// </summary>
    public abstract class BaseRateLimitError : Error
    {
        /// <summary>
        /// When the request can be retried
        /// </summary>
        public DateTime? RetryAfter { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        protected BaseRateLimitError(int? code, string message, Exception? exception) : base(code, message, exception) { }
    }

    /// <summary>
    /// Rate limit exceeded (client side)
    /// </summary>
    public class ClientRateLimitError : BaseRateLimitError
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        public ClientRateLimitError(string message) : base(null, "Client rate limit exceeded: " + message, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        protected ClientRateLimitError(int? code, string message, Exception? exception = null) : base(code, message, exception) { }
    }

    /// <summary>
    /// Rate limit exceeded (server side)
    /// </summary>
    public class ServerRateLimitError : BaseRateLimitError
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ServerRateLimitError(string? message = null, Exception? exception = null) : base(null, "Server rate limit exceeded" + (message?.Length > 0 ? " : " + message : null), exception) { }

        /// <summary>
        /// ctor
        /// </summary>
        protected ServerRateLimitError(int? code, string message, Exception? exception = null) : base(code, message, exception) { }
    }

    /// <summary>
    /// Cancellation requested
    /// </summary>
    public class CancellationRequestedError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancellationRequestedError(Exception? exception = null) : base(null, "Cancellation requested", exception) { }

        /// <summary>
        /// ctor
        /// </summary>
        public CancellationRequestedError(int? code, string message, Exception? exception = null) : base(code, message, exception) { }
    }

    /// <summary>
    /// Invalid operation requested
    /// </summary>
    public class InvalidOperationError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public InvalidOperationError(string message, Exception? exception = null) : base(null, message, exception) { }

        /// <summary>
        /// ctor
        /// </summary>
        protected InvalidOperationError(int? code, string message, Exception? exception = null) : base(code, message, exception) { }
    }
}
