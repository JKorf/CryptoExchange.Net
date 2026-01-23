using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// User data tracker
    /// </summary>
    public interface IUserDataTracker
    {
        /// <summary>
        /// User identifier
        /// </summary>
        public string? UserIdentifier { get; }

        /// <summary>
        /// Current balances
        /// </summary>
        SharedBalance[] Balances { get; }
        /// <summary>
        /// Currently tracked orders
        /// </summary>
        SharedSpotOrder[] Orders { get; }
        /// <summary>
        /// Currently tracked trades
        /// </summary>
        SharedUserTrade[] Trades { get; }

        /// <summary>
        /// On balance update
        /// </summary>
        event Action<UserDataUpdate<SharedBalance[]>>? OnBalanceUpdate;
        /// <summary>
        /// On order update
        /// </summary>
        event Action<UserDataUpdate<SharedSpotOrder[]>>? OnOrderUpdate;
        /// <summary>
        /// On user trade update
        /// </summary>
        event Action<UserDataUpdate<SharedUserTrade[]>>? OnTradeUpdate;

        /// <summary>
        /// Start tracking user data
        /// </summary>
        Task<CallResult> StartAsync();
        /// <summary>
        /// Stop tracking data
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}