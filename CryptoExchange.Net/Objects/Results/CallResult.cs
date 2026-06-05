using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.Objects;

public record CallResult : ICallResult
{
    private static CallResult _successResult = new CallResult();

    /// <inheritdoc />
    public Error? Error { get; init; }
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => Error == null;

    public static CallResult Fail(Error error) => new CallResult { Error = error };
    public static CallResult Ok() => _successResult;
    public static CallResult<T> Ok<T>(T data) => new CallResult<T> { Data = data };
    public static CallResult<T> Fail<T>(Error error) => new CallResult<T> { Error = error };
    /// <inheritdoc />
    public override string ToString()
    {
        return Success ? $"Success" : $"Error: {Error}";
    }
}


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

    public string? OriginalData { get; init; }


    public static CallResult<T> Fail(Error error, string? originalData = null) => new CallResult<T> { Error = error, OriginalData = originalData };
    public static CallResult<T> Ok(T data, string? originalData = null) => new CallResult<T> { Data = data, OriginalData = originalData };
}

public record ExchangeCallResult<T> : CallResult<T>
{
    public string Exchange { get; set; }
    public static ExchangeCallResult<T> Fail(string exchange, Error error) => new ExchangeCallResult<T> { Exchange = exchange, Error = error };
    public static ExchangeCallResult<T> Ok(string exchange, T data) => new ExchangeCallResult<T> { Exchange = exchange, Data = data };
}