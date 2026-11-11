using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces
{
    /// <summary>
    /// Futures user data tracker update handle capability
    /// </summary>
    public interface IFuturesUserDataUpdateHandler
    {
        /// <summary>
        /// Handle a balance update
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="update">Update data</param>
        Task HandleFuturesBalancesUpdateAsync(string userIdentifier, TradingMode tradingMode, UserDataUpdate<SharedBalance[]> update);

        /// <summary>
        /// Handle an order update
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="update">Update data</param>
        Task HandleFuturesOrdersUpdateAsync(string userIdentifier, TradingMode tradingMode, UserDataUpdate<SharedFuturesOrder[]> update);

        /// <summary>
        /// Handle a user trade update
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="update">Update data</param>
        Task HandleFuturesTradesUpdateAsync(string userIdentifier, TradingMode tradingMode, UserDataUpdate<SharedUserTrade[]> update);

        /// <summary>
        /// Handle a position update
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="update">Update data</param>
        Task HandleFuturesPositionsUpdateAsync(string userIdentifier, TradingMode tradingMode, UserDataUpdate<SharedPosition[]> update);
    }
}
