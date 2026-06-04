using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects;

public readonly struct Unit
{
    public static readonly Unit Value = default;
    public static Type Type { get; } = typeof(Unit);
}