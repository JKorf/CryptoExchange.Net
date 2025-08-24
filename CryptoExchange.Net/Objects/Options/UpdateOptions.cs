using CryptoExchange.Net.Authentication;
using System;

namespace CryptoExchange.Net.Objects.Options;

/// <summary>
/// Options to update
/// </summary>
public class UpdateOptions<T> where T : ApiCredentials
{
    /// <summary>
    /// Proxy setting. Note that if this is not provided any previously set proxy will be reset
    /// </summary>
    public ApiProxy? Proxy { get; set; }
    /// <summary>
    /// Api credentials
    /// </summary>
    public T? ApiCredentials { get; set; }
    /// <summary>
    /// Request timeout
    /// </summary>
    public TimeSpan? RequestTimeout { get; set; }
}

/// <inheritdoc />
public class UpdateOptions : UpdateOptions<ApiCredentials> { }
