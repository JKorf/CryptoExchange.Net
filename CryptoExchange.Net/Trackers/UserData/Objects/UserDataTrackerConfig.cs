using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Trackers.UserData.Objects
{
    /// <summary>
    /// User data tracker configuration
    /// </summary>
    public abstract record UserDataTrackerConfig
    {
        /// <summary>
        /// Symbols to initially track, used when polling data. Other symbols will get tracked when updates are received for orders or trades on a new symbol and when there are open orders or positions on a new symbol. To only track the symbols specified here set `OnlyTrackProvidedSymbols` to true.
        /// </summary>
        public IEnumerable<SharedSymbol> TrackedSymbols { get; set; } = [];
        /// <summary>
        /// If true only orders and trades in the `Symbols` options will get tracked, data on other symbols will be ignored.
        /// </summary>
        public bool OnlyTrackProvidedSymbols { get; set; } = false;
        /// <summary>
        /// Whether to track order trades, can lead to increased requests when polling since they're requested per symbol.
        /// </summary>
        public bool TrackTrades { get; set; } = true;
    }

    /// <summary>
    /// Spot user data tracker config
    /// </summary>
    public record SpotUserDataTrackerConfig : UserDataTrackerConfig
    {
        /// <summary>
        /// Balance tracking config
        /// </summary>
        public TrackerItemConfig BalancesConfig { get; set; } = new TrackerItemConfig(true, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        /// <summary>
        /// Order tracking config
        /// </summary>
        public TrackerItemConfig OrdersConfig { get; set; } = new TrackerItemConfig(true, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        /// <summary>
        /// Trade tracking config
        /// </summary>
        public TrackerItemConfig UserTradesConfig { get; set; } = new TrackerItemConfig(false, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }


    /// <summary>
    /// Futures user data tracker config
    /// </summary>
    public record FuturesUserDataTrackerConfig : UserDataTrackerConfig
    {
        /// <summary>
        /// Balance tracking config
        /// </summary>
        public TrackerItemConfig BalancesConfig { get; set; } = new TrackerItemConfig(true, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        /// <summary>
        /// Order tracking config
        /// </summary>
        public TrackerItemConfig OrdersConfig { get; set; } = new TrackerItemConfig(true, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        /// <summary>
        /// Trade tracking config
        /// </summary>
        public TrackerItemConfig UserTradesConfig { get; set; } = new TrackerItemConfig(false, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        /// <summary>
        /// Position tracking config
        /// </summary>
        public TrackerItemConfig PositionConfig { get; set; } = new TrackerItemConfig(true, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }
}
