using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.Objects;

public interface ICallResult
{
    /// <summary>
    /// An error if the call didn't succeed, will always be filled if Success = false
    /// </summary>
    Error? Error { get; }

    /// <summary>
    /// Whether the call was successful
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    bool Success { get; }
}

public interface ICallResult<T> : ICallResult
{
    /// <summary>
    /// An error if the call didn't succeed, will always be filled if Success = false
    /// </summary>
    new Error? Error { get; }

    /// <summary>
    /// Whether the call was successful
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
    new bool Success { get; }
    T? Data { get; }
}