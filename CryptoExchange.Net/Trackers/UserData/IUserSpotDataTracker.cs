using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// User data tracker
    /// </summary>
    public interface IUserSpotDataTracker
    {
        /// <summary>
        /// User identifier
        /// </summary>
        string? UserIdentifier { get; }

        /// <summary>
        /// Whether the tracker is currently fully connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Currently tracked symbols. Data for these symbols will be requested when polling. 
        /// Websocket updates will be available for all symbols regardless.
        /// When new data is received for a symbol which is not yet being tracked it will be added to this list and polled in the future unless the `OnlyTrackProvidedSymbols` option is set in the configuration.
        /// </summary>
        IEnumerable<SharedSymbol> TrackedSymbols { get; }

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
        /// On connection status change. Might trigger multiple times with the same status depending on the underlying subscriptions.
        /// </summary>
        event Action<bool>? OnConnectedStatusChange;
        /// <summary>
        /// On balance update
        /// </summary>
        event Func<UserDataUpdate<SharedBalance[]>, Task>? OnBalanceUpdate;
        /// <summary>
        /// On order update
        /// </summary>
        event Func<UserDataUpdate<SharedSpotOrder[]>, Task>? OnOrderUpdate;
        /// <summary>
        /// On user trade update
        /// </summary>
        event Func<UserDataUpdate<SharedUserTrade[]>, Task>? OnTradeUpdate;

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