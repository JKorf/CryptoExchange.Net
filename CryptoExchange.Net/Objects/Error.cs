﻿namespace CryptoExchange.Net.Objects
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
        public CantConnectError() : base(null, "Can't connect to the server", null) { }
    }

    /// <summary>
    /// No api credentials provided while trying to access private endpoint
    /// </summary>
    public class NoApiCredentialsError : Error
    {
        /// <summary>
        /// ctor
        /// </summary>
        public NoApiCredentialsError() : base(null, "No credentials provided for private endpoint", null) { }
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
        public ServerError(string message, object? data = null) : base(null, message, data) { }

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
        public RateLimitError(string message) : base(null, "Rate limit exceeded: " + message, null) { }
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
    }
}
