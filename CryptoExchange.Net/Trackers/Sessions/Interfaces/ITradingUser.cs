using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces;

/// <summary>
/// User access within a trading strategy
/// </summary>
public interface ITradingUser
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
    /// The registered spot user data tracker
    /// </summary>
    IUserSpotDataTracker SpotUserDataTracker { get; }

    /// <summary>
    /// Get the registered futures user data tracker
    /// </summary>
    IUserFuturesDataTracker GetFuturesUserDataTracker(TradingMode tradingMode);

    bool Supports<TCapability>(SharedCapabilityReference<TCapability> capability, TradingMode tradingMode)
        where TCapability : ISharedApiCapability;

    CapabilityOptions? GetOptions<TCapability>(
        SharedCapabilityReference<TCapability> capability,
        TradingMode tradingMode)
        where TCapability : ISharedApiCapability;
}
