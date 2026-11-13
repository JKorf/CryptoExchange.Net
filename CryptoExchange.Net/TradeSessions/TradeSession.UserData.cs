using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions;

public abstract partial class TradeSession
{
    /// <summary>
    /// Handle a balance update
    /// </summary>
    /// <param name="userIdentifier">User identifier</param>
    /// <param name="update">Update data</param>
    protected virtual Task HandleSpotBalancesUpdateAsync(
        string userIdentifier,
        UserDataUpdate<SharedBalance[]> update) => Task.CompletedTask;

    /// <summary>
    /// Handle an order update
    /// </summary>
    /// <param name="userIdentifier">User identifier</param>
    /// <param name="update">Update data</param>
    protected virtual Task HandleSpotOrdersUpdateAsync(
        string userIdentifier,
        UserDataUpdate<SharedSpotOrder[]> update) => Task.CompletedTask;

    /// <summary>
    /// Handle a user trade update
    /// </summary>
    /// <param name="userIdentifier">User identifier</param>
    /// <param name="update">Update data</param>
    protected virtual Task HandleSpotTradesUpdateAsync(
        string userIdentifier,
        UserDataUpdate<SharedUserTrade[]> update) => Task.CompletedTask;

    /// <summary>
    /// Handle a balance update
    /// </summary>
    /// <param name="userIdentifier">User identifier</param>
    /// <param name="tradingMode">Trading mode</param>
    /// <param name="update">Update data</param>
    protected virtual Task HandleFuturesBalancesUpdateAsync(
        string userIdentifier,
        TradingMode tradingMode,
        UserDataUpdate<SharedBalance[]> update) => Task.CompletedTask;

    /// <summary>
    /// Handle an order update
    /// </summary>
    /// <param name="userIdentifier">User identifier</param>
    /// <param name="tradingMode">Trading mode</param>
    /// <param name="update">Update data</param>
    protected virtual Task HandleFuturesOrdersUpdateAsync(
        string userIdentifier,
        TradingMode tradingMode,
        UserDataUpdate<SharedFuturesOrder[]> update) => Task.CompletedTask;

    /// <summary>
    /// Handle a user trade update
    /// </summary>
    /// <param name="userIdentifier">User identifier</param>
    /// <param name="tradingMode">Trading mode</param>
    /// <param name="update">Update data</param>
    protected virtual Task HandleFuturesTradesUpdateAsync(
        string userIdentifier,
        TradingMode tradingMode,
        UserDataUpdate<SharedUserTrade[]> update) => Task.CompletedTask;

    /// <summary>
    /// Handle a position update
    /// </summary>
    /// <param name="userIdentifier">User identifier</param>
    /// <param name="tradingMode">Trading mode</param>
    /// <param name="update">Update data</param>
    protected virtual Task HandleFuturesPositionsUpdateAsync(
        string userIdentifier,
        TradingMode tradingMode,
        UserDataUpdate<SharedPosition[]> update) => Task.CompletedTask;

    internal Task InvokeSpotBalancesUpdateAsync(string userIdentifier, UserDataUpdate<SharedBalance[]> update)
        => HandleSpotBalancesUpdateAsync(userIdentifier, update);

    internal Task InvokeSpotOrdersUpdateAsync(string userIdentifier, UserDataUpdate<SharedSpotOrder[]> update)
        => HandleSpotOrdersUpdateAsync(userIdentifier, update);

    internal Task InvokeSpotTradesUpdateAsync(string userIdentifier, UserDataUpdate<SharedUserTrade[]> update)
        => HandleSpotTradesUpdateAsync(userIdentifier, update);

    internal Task InvokeFuturesBalancesUpdateAsync(
        string userIdentifier,
        TradingMode tradingMode,
        UserDataUpdate<SharedBalance[]> update)
        => HandleFuturesBalancesUpdateAsync(userIdentifier, tradingMode, update);

    internal Task InvokeFuturesOrdersUpdateAsync(
        string userIdentifier,
        TradingMode tradingMode,
        UserDataUpdate<SharedFuturesOrder[]> update)
        => HandleFuturesOrdersUpdateAsync(userIdentifier, tradingMode, update);

    internal Task InvokeFuturesTradesUpdateAsync(
        string userIdentifier,
        TradingMode tradingMode,
        UserDataUpdate<SharedUserTrade[]> update)
        => HandleFuturesTradesUpdateAsync(userIdentifier, tradingMode, update);

    internal Task InvokeFuturesPositionsUpdateAsync(
        string userIdentifier,
        TradingMode tradingMode,
        UserDataUpdate<SharedPosition[]> update)
        => HandleFuturesPositionsUpdateAsync(userIdentifier, tradingMode, update);
}
