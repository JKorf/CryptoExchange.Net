using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.Objects;

/// <summary>
/// Call result
/// </summary>
public record CallResult : ICallResult
{
    private static CallResult _successResult = new CallResult();

    /// <inheritdoc />
    public Error? Error { get; init; }
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => Error == null;

    /// <summary>
    /// Create an error response
    /// </summary>
    /// <param name="error">The error</param>
    public static CallResult Fail(Error error) => new CallResult { Error = error };
    /// <summary>
    /// Create a success result
    /// </summary>
    public static CallResult Ok() => _successResult;
    /// <summary>
    /// Create a success result
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="originalData">The original string data</param>
    /// <param name="data">Data type</param>
    public static CallResult<T> Ok<T>(T data, string? originalData = null) => new CallResult<T> { Data = data, OriginalData = originalData };
    /// <summary>
    /// Create an error response
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    /// <param name="originalData">The original string data</param>
    /// <param name="error">The error</param>
    public static CallResult<T> Fail<T>(Error error, string? originalData = null) => new CallResult<T> { Error = error, OriginalData = originalData };

    /// <inheritdoc />
    public override string ToString()
    {
        return Success ? $"Success" : $"Error: {Error}";
    }
}


/// <inheritdoc />
public record CallResult<T> : CallResult, ICallResult<T>
{
    /// <inheritdoc />
    public new Error? Error
    {
        get => base.Error;
        init => base.Error = value;
    }
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
    public new bool Success => Error == null;

    /// <summary>
    /// The data returned by the call, only available when Success = true
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// The original data returned by the call, only available when `OutputOriginalData` is set to `true` in the client options
    /// </summary>
    public string? OriginalData { get; init; }

    /// <summary>
    /// Create an error response
    /// </summary>
    /// <param name="error">The error</param>
    /// <param name="originalData">The original string data</param>
    public static CallResult<T> Fail(Error error, string? originalData = null) => new CallResult<T> { Error = error, OriginalData = originalData };
    /// <summary>
    /// Create a success result
    /// </summary>
    /// <param name="data">The data</param>
    /// <param name="originalData">The original string data</param>
    /// <returns></returns>
    public static CallResult<T> Ok(T data, string? originalData = null) => new CallResult<T> { Data = data, OriginalData = originalData };
}

/// <summary>
/// Call result for an exchange
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public record ExchangeCallResult<T> : CallResult<T>
{
    /// <summary>
    /// Exchange name
    /// </summary>
    public string Exchange { get; set; } = string.Empty;
    /// <summary>
    /// Create an error response
    /// </summary>
    /// <param name="exchange">The exchange name</param>
    /// <param name="error">The error</param>
    /// <param name="originalData">The original string data</param>
    public static ExchangeCallResult<T> Fail(string exchange, Error error, string? originalData = null) => new ExchangeCallResult<T> { Exchange = exchange, Error = error };
    /// <summary>
    /// Create a success result
    /// </summary>
    /// <param name="exchange">The exchange name</param>
    /// <param name="data">The data</param>
    /// <param name="originalData">The original string data</param>
    /// <returns></returns>
    public static ExchangeCallResult<T> Ok(string exchange, T data, string? originalData = null) => new ExchangeCallResult<T> { Exchange = exchange, Data = data };
}