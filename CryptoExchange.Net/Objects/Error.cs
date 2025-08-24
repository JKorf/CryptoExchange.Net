using CryptoExchange.Net.Objects.Errors;
using System;

namespace CryptoExchange.Net.Objects;

/// <summary>
/// Base class for errors
/// </summary>
public abstract class Error
{
    private int? _code;
    /// <summary>
    /// The int error code the server returned; or the http status code int value if there was no error code.<br />
    /// <br />
    /// <i>Note:</i><br />
    /// The <see cref="ErrorCode"/> property should be used for more generic error checking; it might contain a string error code if the server does not return an int code.
    /// </summary>
    public int? Code 
    { 
        get
        {
            if (_code.HasValue)
                return _code;

            return int.TryParse(ErrorCode, out var r) ? r : null;
        }
        set
        {
            _code = value;
        }
    }

    /// <summary>
    /// The error code returned by the server
    /// </summary>
    public string? ErrorCode { get; set; }
    /// <summary>
    /// The error description
    /// </summary>
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Error type
    /// </summary>
    public ErrorType ErrorType { get; set; }

    /// <summary>
    /// Whether the error is transient and can be retried
    /// </summary>
    public bool IsTransient { get; set; }

    /// <summary>
    /// The server message for the error that occurred
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Underlying exception
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    protected Error(string? errorCode, ErrorInfo errorInfo, Exception? exception)
    {
        ErrorCode = errorCode;
        ErrorType = errorInfo.ErrorType;
        Message = errorInfo.Message;
        ErrorDescription = errorInfo.ErrorDescription;
        IsTransient = errorInfo.IsTransient;
        Exception = exception;
    }

    /// <summary>
    /// String representation
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ErrorCode != null ? $"[{GetType().Name}.{ErrorType}] {ErrorCode}: {Message ?? ErrorDescription}" : $"[{GetType().Name}.{ErrorType}] {Message ?? ErrorDescription}";
    }
}

/// <summary>
/// Cant reach server error
/// </summary>
public class CantConnectError : Error
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.UnableToConnect, false, "Can't connect to the server");

    /// <summary>
    /// ctor
    /// </summary>
    public CantConnectError() : base(null, errorInfo, null) { }

    /// <summary>
    /// ctor
    /// </summary>
    public CantConnectError(Exception? exception) : base(null, errorInfo, exception) { }

    /// <summary>
    /// ctor
    /// </summary>
    protected CantConnectError(ErrorInfo info, Exception? exception) : base(null, info, exception) { }
}

/// <summary>
/// No api credentials provided while trying to access a private endpoint
/// </summary>
public class NoApiCredentialsError : Error
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.MissingCredentials, false, "No credentials provided for private endpoint");

    /// <summary>
    /// ctor
    /// </summary>
    public NoApiCredentialsError() : base(null, errorInfo, null) { }

    /// <summary>
    /// ctor
    /// </summary>
    protected NoApiCredentialsError(ErrorInfo info, Exception? exception) : base(null, info, exception) { }
}

/// <summary>
/// Error returned by the server
/// </summary>
public class ServerError : Error
{
    /// <summary>
    /// ctor
    /// </summary>
    public ServerError(ErrorInfo errorInfo, Exception? exception = null)
         : base(null, errorInfo, exception) { }

    /// <summary>
    /// ctor
    /// </summary>
    public ServerError(int errorCode, ErrorInfo errorInfo, Exception? exception = null) 
        : this(errorCode.ToString(), errorInfo, exception) { }

    /// <summary>
    /// ctor
    /// </summary>
    public ServerError(string errorCode, ErrorInfo errorInfo, Exception? exception = null) : base(errorCode, errorInfo, exception) { }
}

/// <summary>
/// Web error returned by the server
/// </summary>
public class WebError : Error
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.NetworkError, true, "Failed to complete the request to the server due to a network error");

    /// <summary>
    /// ctor
    /// </summary>
    public WebError(string? message = null, Exception? exception = null) : base(null, errorInfo with { Message = (message?.Length > 0 ? errorInfo.Message + ": " + message : errorInfo.Message) }, exception) { }
}

/// <summary>
/// Timeout error waiting for a response from the server
/// </summary>
public class TimeoutError : Error
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.Timeout, false, "Failed to receive a response from the server in time");

    /// <summary>
    /// ctor
    /// </summary>
    public TimeoutError(string? message = null, Exception? exception = null) : base(null, errorInfo with { Message = (message?.Length > 0 ? errorInfo.Message + ": " + message : errorInfo.Message) }, exception) { }
}

/// <summary>
/// Error while deserializing data
/// </summary>
public class DeserializeError : Error
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.DeserializationFailed, false, "Failed to deserialize data");

    /// <summary>
    /// ctor
    /// </summary>
    public DeserializeError(string? message = null, Exception? exception = null) : base(null, errorInfo with { Message = (message?.Length > 0 ? errorInfo.Message + ": " + message : errorInfo.Message) }, exception) { }
}

/// <summary>
/// An invalid parameter has been provided
/// </summary>
public class ArgumentError : Error
{
    /// <summary>
    /// Default error info for missing parameter
    /// </summary>
    protected static readonly ErrorInfo missingInfo = new ErrorInfo(ErrorType.MissingParameter, false, "Missing parameter");
    /// <summary>
    /// Default error info for invalid parameter
    /// </summary>
    protected static readonly ErrorInfo invalidInfo = new ErrorInfo(ErrorType.InvalidParameter, false, "Invalid parameter");

    /// <summary>
    /// ctor
    /// </summary>
    public static ArgumentError Missing(string parameterName, string? message = null) => new ArgumentError(missingInfo with { Message = message == null ? $"{missingInfo.Message} '{parameterName}'" : $"{missingInfo.Message} '{parameterName}': {message}" }, null);

    /// <summary>
    /// ctor
    /// </summary>
    public static ArgumentError Invalid(string parameterName, string message) => new ArgumentError(invalidInfo with { Message = $"{invalidInfo.Message} '{parameterName}': {message}" }, null);

    /// <summary>
    /// ctor
    /// </summary>
    protected ArgumentError(ErrorInfo info, Exception? exception) : base(null, info, exception) { }
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
    protected BaseRateLimitError(ErrorInfo errorInfo, Exception? exception) : base(null, errorInfo, exception) { }
}

/// <summary>
/// Rate limit exceeded (client side)
/// </summary>
public class ClientRateLimitError : BaseRateLimitError
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.RateLimitRequest, false, "Client rate limit exceeded");

    /// <summary>
    /// ctor
    /// </summary>
    public ClientRateLimitError(string? message = null, Exception? exception = null) : base(errorInfo with { Message = (message?.Length > 0 ? errorInfo.Message + ": " + message : errorInfo.Message) }, exception) { }

    /// <summary>
    /// ctor
    /// </summary>
    protected ClientRateLimitError(ErrorInfo info, Exception? exception) : base(info, exception) { }
}

/// <summary>
/// Rate limit exceeded (server side)
/// </summary>
public class ServerRateLimitError : BaseRateLimitError
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.RateLimitRequest, false, "Server rate limit exceeded");

    /// <summary>
    /// ctor
    /// </summary>
    public ServerRateLimitError(string? message = null, Exception? exception = null) : base(errorInfo with { Message = (message?.Length > 0 ? errorInfo.Message + ": " + message : errorInfo.Message) }, exception) { }

    /// <summary>
    /// ctor
    /// </summary>
    protected ServerRateLimitError(ErrorInfo info, Exception? exception) : base(info, exception) { }
}

/// <summary>
/// Cancellation requested
/// </summary>
public class CancellationRequestedError : Error
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.CancellationRequested, false, "Cancellation requested");

    /// <summary>
    /// ctor
    /// </summary>
    public CancellationRequestedError(Exception? exception = null) : base(null, errorInfo, null) { }

    /// <summary>
    /// ctor
    /// </summary>
    protected CancellationRequestedError(ErrorInfo info, Exception? exception) : base(null, info, exception) { }
}

/// <summary>
/// Invalid operation requested
/// </summary>
public class InvalidOperationError : Error
{
    /// <summary>
    /// Default error info
    /// </summary>
    protected static readonly ErrorInfo errorInfo = new ErrorInfo(ErrorType.InvalidOperation, false, "Operation invalid");

    /// <summary>
    /// ctor
    /// </summary>
    public InvalidOperationError(string message) : base(null, errorInfo with { Message = message }, null) { }

    /// <summary>
    /// ctor
    /// </summary>
    protected InvalidOperationError(ErrorInfo info, Exception? exception) : base(null, info, exception) { }
}
