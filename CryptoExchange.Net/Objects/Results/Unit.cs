using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects;

/// <summary>
/// Void result
/// </summary>
public readonly struct Unit
{
    /// <summary>
    /// Void value
    /// </summary>
    public static readonly Unit Value = default;
    /// <summary>
    /// Type
    /// </summary>
    public static Type Type { get; } = typeof(Unit);
}