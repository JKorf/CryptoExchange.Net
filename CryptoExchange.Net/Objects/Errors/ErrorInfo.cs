using System;

namespace CryptoExchange.Net.Objects.Errors
{
    /// <summary>
    /// Error info
    /// </summary>
    public record ErrorInfo
    {
        /// <summary>
        /// Unknown error info
        /// </summary>
        public static ErrorInfo Unknown { get; } = new ErrorInfo(ErrorType.Unknown, false, "Unknown error", []);

        /// <summary>
        /// The server error code
        /// </summary>
        public string[] ErrorCodes { get; set; }
        /// <summary>
        /// Error description
        /// </summary>
        public string? ErrorDescription { get; set; }
        /// <summary>
        /// The error type
        /// </summary>
        public ErrorType ErrorType { get; set; }
        /// <summary>
        /// Whether the error is transient and can be retried
        /// </summary>
        public bool IsTransient { get; set; }
        /// <summary>
        /// Server response message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public ErrorInfo(ErrorType errorType, string description)
        {
            ErrorCodes = [];
            ErrorType = errorType;
            IsTransient = false;
            ErrorDescription = description;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ErrorInfo(ErrorType errorType, bool isTransient, string description, params string[] errorCodes)
        {
            ErrorCodes = errorCodes;
            ErrorType = errorType;
            IsTransient = isTransient;
            ErrorDescription = description;
        }
    }
}
