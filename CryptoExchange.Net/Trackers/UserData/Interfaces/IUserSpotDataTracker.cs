using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData.Interfaces
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
        IUserDataTracker<SharedSpotOrder> Orders { get; }
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

        /// <summary>
        /// Add symbols to the list of symbols for which data is being tracked
        /// </summary>
        /// <param name="symbols">Symbols to add</param>
        void AddTrackedSymbolsAsync(IEnumerable<SharedSymbol> symbols);

        /// <summary>
        /// Remove a symbol from the list of symbols for which data is being tracked. 
        /// Note that the symbol will be added again if new data for that symbol is received, unless the OnlyTrackProvidedSymbols option has been set to true.
        /// </summary>
        /// <param name="symbol">Symbol to remove</param>
        void RemoveTrackedSymbolAsync(SharedSymbol symbol);
    }
}