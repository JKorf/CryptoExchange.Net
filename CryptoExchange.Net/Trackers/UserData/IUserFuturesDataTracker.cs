using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// Futures user data tracker
    /// </summary>
    public interface IUserFuturesDataTracker
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
        SharedFuturesOrder[] Orders { get; }
        /// <summary>
        /// Currently tracked positions
        /// </summary>
        SharedPosition[] Positions { get; }
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
        event Func<UserDataUpdate<SharedFuturesOrder[]>, Task>? OnOrderUpdate;
        /// <summary>
        /// On position order update
        /// </summary>
        event Func<UserDataUpdate<SharedPosition[]>, Task>? OnPositionUpdate;
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