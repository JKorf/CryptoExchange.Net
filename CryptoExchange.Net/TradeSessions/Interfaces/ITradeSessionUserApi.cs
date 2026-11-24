using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;

namespace CryptoExchange.Net.TradeSessions.Interfaces;

/// <summary>
/// User-bound access to a specific exchange API surface
/// </summary>
public interface ITradeSessionUserApi
{
    /// <summary>
    /// User identifier
    /// </summary>
    string UserIdentifier { get; }

    /// <summary>
    /// Trading environment
    /// </summary>
    TradeEnvironment? Environment { get; }

    /// <summary>
    /// Get whether the capability is supported
    /// </summary>
    bool Supports<TCapability>(SharedCapabilityReference<TCapability> capability, TradingMode? tradingMode = null)
        where TCapability : ISharedApiCapability;

    /// <summary>
    /// Get the options for a capability
    /// </summary>
    CapabilityOptions? GetOptions<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode? tradingMode = null)
        where TCapability : ISharedApiCapability;
}
