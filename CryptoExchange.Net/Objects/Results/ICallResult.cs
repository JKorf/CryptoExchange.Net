using System;
using System.Collections.Generic;
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
    bool Success { get; }
}

public interface ICallResult<T> : ICallResult
{
    T Data { get; }
}