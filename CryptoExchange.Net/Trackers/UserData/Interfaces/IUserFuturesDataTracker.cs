using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData.Interfaces
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
        /// Exchange name
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Currently tracked symbols. Data for these symbols will be requested when polling. 
        /// Websocket updates will be available for all symbols regardless.
        /// When new data is received for a symbol which is not yet being tracked it will be added to this list and polled in the future unless the `OnlyTrackProvidedSymbols` option is set in the configuration.
        /// </summary>
        IEnumerable<SharedSymbol> TrackedSymbols { get; }

        /// <summary>
        /// Balances tracker
        /// </summary>
        IUserDataTracker<SharedBalance> Balances { get; }
        /// <summary>
        /// Orders tracker
        /// </summary>
        IUserDataTracker<SharedFuturesOrder> Orders { get; }
        /// <summary>
        /// Positions tracker
        /// </summary>
        IUserDataTracker<SharedPosition> Positions { get; }
        /// <summary>
        /// Trades tracker
        /// </summary>
        IUserDataTracker<SharedUserTrade>? Trades { get; }

        /// <summary>
        /// On connection status change
        /// </summary>
        event Action<UserDataType, bool>? OnConnectedChange;

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