using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.UserData.Interfaces
{
    /// <summary>
    /// Data tracker interface
    /// </summary>
    public interface IUserDataTracker<T>
    {
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
        /// On connection status change. Might trigger multiple times with the same status depending on the underlying subscriptions.
        /// </summary>
        event Action<bool>? OnConnectedChange;

        /// <summary>
        /// Currently tracker values
        /// </summary>
        T[] Values { get; }

        /// <summary>
        /// On data update
        /// </summary>
        event Func<UserDataUpdate<T[]>, Task>? OnUpdate;
    }
}
