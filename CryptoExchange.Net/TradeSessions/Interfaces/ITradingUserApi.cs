using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;

namespace CryptoExchange.Net.TradeSessions.Interfaces;

/// <summary>
/// User-bound access to a specific exchange API surface
/// </summary>
public interface ITradingUserApi
{
    /// <summary>
    /// User identifier
    /// </summary>
    string UserIdentifier { get; }

    /// <summary>
    /// Trading environment
    /// </summary>
    TradeEnvironment? Environment { get; }

    bool Supports<TCapability>(SharedCapabilityReference<TCapability> capability, TradingMode? tradingMode = null)
        where TCapability : ISharedApiCapability;

    CapabilityOptions? GetOptions<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode? tradingMode = null)
        where TCapability : ISharedApiCapability;
}
