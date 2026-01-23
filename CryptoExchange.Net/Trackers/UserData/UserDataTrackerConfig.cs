using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Trackers.UserData
{
    /// <summary>
    /// User data tracker configuration
    /// </summary>
    public record UserDataTrackerConfig
    {
        /// <summary>
        /// Symbols to initially track, used when polling data. Other symbols will get tracked when updates are received for orders or trades on a new symbol and when there are open orders on a new symbol. To only track the symbols specified here set `OnlyTrackProvidedSymbols` to true.
        /// </summary>
        public IEnumerable<SharedSymbol> Symbols { get; set; } = [];
        /// <summary>
        /// If true only orders and trades in the `Symbols` options will get tracked, data on other symbols will be ignored.
        /// </summary>
        public bool OnlyTrackProvidedSymbols { get; set; } = false;
        /// <summary>
        /// Interval to poll data at as backup, even when the websocket stream is still connected.
        /// </summary>
        public TimeSpan PollIntervalConnected { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// Interval to poll data at while the websocket is disconnected.
        /// </summary>
        public TimeSpan PollIntervalDisconnected { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// Whether to poll for data initially when starting the tracker.
        /// </summary>
        public bool PollAtStart { get; set; } = true;
    }
}
