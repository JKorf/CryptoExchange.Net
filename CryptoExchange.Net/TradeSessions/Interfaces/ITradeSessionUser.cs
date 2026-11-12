using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;

namespace CryptoExchange.Net.TradeSessions.Interfaces;

/// <summary>
/// User access within a trade session
/// </summary>
public interface ITradeSessionUser
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
    /// Get the registered spot user data tracker
    /// </summary>
    IUserSpotDataTracker GetSpotUserDataTracker();

    /// <summary>
    /// Get the registered futures user data tracker
    /// </summary>
    IUserFuturesDataTracker GetFuturesUserDataTracker(TradingMode tradingMode);

}
