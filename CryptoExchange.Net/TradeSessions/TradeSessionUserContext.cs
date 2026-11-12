using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using System;

namespace CryptoExchange.Net.TradeSessions;

/// <summary>
/// Trade session user context
/// </summary>
public sealed class TradeSessionUserContext
{
    private readonly Func<IUserSpotDataTracker> _getSpotTracker;
    private readonly Func<TradingMode, IUserFuturesDataTracker> _getFuturesTracker;

    /// <summary>
    /// The user definition
    /// </summary>
    public TradeSessionUserDefinition Definition { get; }

    internal TradeSessionUserContext(
        TradeSessionUserDefinition definition,
        Func<IUserSpotDataTracker> getSpotTracker,
        Func<TradingMode, IUserFuturesDataTracker> getFuturesTracker)
    {
        Definition = definition;
        _getSpotTracker = getSpotTracker;
        _getFuturesTracker = getFuturesTracker;
    }

    /// <summary>
    /// Get spot user data tracker
    /// </summary>
    public IUserSpotDataTracker GetSpotUserDataTracker() => _getSpotTracker();

    /// <summary>
    /// Get futures user data tracker
    /// </summary>
    public IUserFuturesDataTracker GetFuturesUserDataTracker(TradingMode mode) => _getFuturesTracker(mode);
}
