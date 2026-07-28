using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Ticker info
    /// </summary>
    [DebuggerDisplay("{Symbol,nq} High: {HighPrice}, Low: {LowPrice}, Last: {LastPrice}, Change: {ChangePercentage}%")]
    public record SharedSpotTicker: SharedSymbolModel
    {
        /// <summary>
        /// Last trade price
        /// </summary>
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// Highest price in last 24h
        /// </summary>
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// Lowest price in last 24h
        /// </summary>
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// The volume in the last 24h
        /// </summary>
        public SharedOrderQuantity Volumes { get; set; }

        private decimal? _volume;
        /// <summary>
        /// Trade volume in base asset in the last 24h
        /// </summary>
        [Obsolete("Use `Volumes` instead")]
        public decimal Volume
        {
            get
            {
                if (_volume.HasValue)
                    return _volume.Value;

                return Volumes.QuantityInBaseAsset ?? Volumes.QuantityInContracts ?? 0;
            }
            set => _volume = value;
        }
        /// <summary>
        /// Trade volume in quote asset in the last 24h
        /// </summary>
        [Obsolete("Use `Volumes` instead")]
        public decimal? QuoteVolume => Volumes?.QuantityInQuoteAsset;
        /// <summary>
        /// Change percentage in the last 24h
        /// </summary>
        public decimal? ChangePercentage { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSpotTicker(
            SharedSymbol? sharedSymbol,
            string symbol,
            decimal? lastPrice,
            decimal? highPrice,
            decimal? lowPrice,
            SharedOrderQuantity volumes,
            decimal? changePercentage)
            : base(sharedSymbol, symbol)
        {
            LastPrice = lastPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Volumes = volumes;
            ChangePercentage = changePercentage;
        }
    }
}
