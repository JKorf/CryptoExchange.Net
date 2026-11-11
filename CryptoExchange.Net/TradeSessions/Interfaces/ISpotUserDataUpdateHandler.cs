using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Interfaces
{
    /// <summary>
    /// Spot user data tracker update handle capability
    /// </summary>
    public interface ISpotUserDataUpdateHandler
    {
        /// <summary>
        /// Handle a balance update
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="update">Update data</param>
        Task HandleSpotBalancesUpdateAsync(string userIdentifier, UserDataUpdate<SharedBalance[]> update);

        /// <summary>
        /// Handle an order update
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="update">Update data</param>
        Task HandleSpotOrdersUpdateAsync(string userIdentifier, UserDataUpdate<SharedSpotOrder[]> update);

        /// <summary>
        /// Handle a user trade update
        /// </summary>
        /// <param name="userIdentifier">User identifier</param>
        /// <param name="update">Update data</param>
        Task HandleSpotTradesUpdateAsync(string userIdentifier, UserDataUpdate<SharedUserTrade[]> update);
    }
}
