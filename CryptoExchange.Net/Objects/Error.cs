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
        /// The data which caused the error
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected Error(int? code, string message, object? data)
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
            return Code != null ? $"[{GetType().Name}] {Code}: {Message} {Data}" : $"[{GetType().Name}] {Message} {Data}";
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
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected CantConnectError(int? code, string message, object? data) : base(code, message, data) { }
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
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected NoApiCredentialsError(int? code, string message, object? data) : base(code, message, data) { }
    }

    /// <summary>
    /// Error returned by the server
    /// </summary>
    public class ServerError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        public ServerError(string message, object? data = null) : base(null, message, data) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        public ServerError(int code, string message, object? data = null) : base(code, message, data) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected ServerError(int? code, string message, object? data) : base(code, message, data) { }
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
        /// <param name="data"></param>
        public WebError(string message, object? data = null) : base(null, message, data) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        public WebError(int code, string message, object? data = null) : base(code, message, data) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected WebError(int? code, string message, object? data): base(code, message, data) { }
    }

    /// <summary>
    /// Error while deserializing data
    /// </summary>
    public class DeserializeError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="data">The data which caused the error</param>
        public DeserializeError(string message, object? data) : base(null, message, data) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected DeserializeError(int? code, string message, object? data): base(code, message, data) { }
    }

    /// <summary>
    /// Unknown error
    /// </summary>
    public class UnknownError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="data">Error data</param>
        public UnknownError(string message, object? data = null) : base(null, message, data) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected UnknownError(int? code, string message, object? data): base(code, message, data) { }
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
        public ArgumentError(string message) : base(null, "Invalid parameter: " + message, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected ArgumentError(int? code, string message, object? data): base(code, message, data) { }
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
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected BaseRateLimitError(int? code, string message, object? data) : base(code, message, data) { }
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
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected ClientRateLimitError(int? code, string message, object? data): base(code, message, data) { }
    }

    /// <summary>
    /// Rate limit exceeded (server side)
    /// </summary>
    public class ServerRateLimitError : BaseRateLimitError
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        public ServerRateLimitError(string message) : base(null, "Server rate limit exceeded: " + message, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected ServerRateLimitError(int? code, string message, object? data) : base(code, message, data) { }
    }

    /// <summary>
    /// Cancellation requested
    /// </summary>
    public class CancellationRequestedError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancellationRequestedError() : base(null, "Cancellation requested", null) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        public CancellationRequestedError(int? code, string message, object? data): base(code, message, data) { }
    }

    /// <summary>
    /// Invalid operation requested
    /// </summary>
    public class InvalidOperationError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        public InvalidOperationError(string message) : base(null, message, null) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        protected InvalidOperationError(int? code, string message, object? data): base(code, message, data) { }
    }
}
