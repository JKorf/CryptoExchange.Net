using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using System;

namespace CryptoExchange.Net.Trackers.Sessions;

public sealed class TradingSessionUserContext
{
    private readonly Func<IUserSpotDataTracker> _getSpotTracker;
    private readonly Func<TradingMode, IUserFuturesDataTracker> _getFuturesTracker;

    public TradingSessionUserDefinition Definition { get; }

    internal TradingSessionUserContext(
        TradingSessionUserDefinition definition,
        Func<IUserSpotDataTracker> getSpotTracker,
        Func<TradingMode, IUserFuturesDataTracker> getFuturesTracker)
    {
        Definition = definition;
        _getSpotTracker = getSpotTracker;
        _getFuturesTracker = getFuturesTracker;
    }

    public IUserSpotDataTracker GetSpotUserDataTracker() => _getSpotTracker();

    public IUserFuturesDataTracker GetFuturesUserDataTracker(TradingMode mode) => _getFuturesTracker(mode);
}
