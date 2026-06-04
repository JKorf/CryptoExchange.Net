using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.Objects;

public record CallResult<T> : ICallResult
{
    /// <inheritdoc />
    public Error? Error { get; init; }
    /// <inheritdoc />
#if NET5_0_OR_GREATER
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
#endif
    public bool Success => Error == null;

    /// <summary>
    /// The data returned by the call, only available when Success = true
    /// </summary>
    public T? Data { get; init; }

    public static CallResult<T> Fail(Error error) => new CallResult<T> { Error = error };
    public static CallResult<T> Ok(T data) => new CallResult<T> { Data = data };

    /// <inheritdoc />
    public override string ToString()
    {
        return Success ? $"Success" : $"Error: {Error}";
    }
}


public record CallResult : CallResult<Unit>
{
    private static CallResult _successResult = new CallResult();

    public static CallResult Fail(Error error) => new CallResult { Error = error };
    public static CallResult<T> Fail<T>(Error error) => new CallResult<T> { Error = error };
    public static CallResult Ok() => _successResult;
    public static CallResult<T> Ok<T>(T data) => new CallResult<T> { Data = data };
}

public record ExchangeCallResult<T> : CallResult<T>
{
    public string Exchange { get; set; }
    public static ExchangeCallResult<T> Fail(string exchange, Error error) => new ExchangeCallResult<T> { Exchange = exchange, Error = error };
    public static ExchangeCallResult<T> Ok(string exchange, T data) => new ExchangeCallResult<T> { Exchange = exchange, Data = data };
}