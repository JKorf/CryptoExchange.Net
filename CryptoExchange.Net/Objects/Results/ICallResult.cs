using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoExchange.Net.Objects;

/// <summary>
/// Call result
/// </summary>
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

/// <summary>
/// Call result
/// </summary>
/// <typeparam name="T">Result data type</typeparam>
public interface ICallResult<T> : ICallResult
{
    /// <inheritdoc />
    new Error? Error { get; }

    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Error))]
    [MemberNotNullWhen(true, nameof(Data))]
    new bool Success { get; }

    /// <summary>
    /// The result data, only available when Success = true
    /// </summary>
    T? Data { get; }
}