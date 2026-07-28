using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Futures ticker info
    /// </summary>
    [DebuggerDisplay("{Symbol,nq} High: {HighPrice}, Low: {LowPrice}, Last: {LastPrice}, Change: {ChangePercentage}%")]
    public record SharedFuturesTicker: SharedSymbolModel
    {
        /// <summary>
        /// Last trade price
        /// </summary>
        public decimal? LastPrice { get; set; }
        /// <summary>
        /// High price in the last 24h
        /// </summary>
        public decimal? HighPrice { get; set; }
        /// <summary>
        /// Low price in the last 24h
        /// </summary>
        public decimal? LowPrice { get; set; }
        /// <summary>
        /// The volume in the last 24h
        /// </summary>
        public SharedOrderQuantity Volumes { get; set; }

        private decimal? _volume;
        /// <summary>
        /// The volume in the last 24h
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
        /// Change percentage in the last 24h
        /// </summary>
        public decimal? ChangePercentage { get; set; }
        /// <summary>
        /// Current mark price
        /// </summary>
        public decimal? MarkPrice { get; set; }
        /// <summary>
        /// Current index price
        /// </summary>
        public decimal? IndexPrice { get; set; }
        /// <summary>
        /// Current funding rate
        /// </summary>
        public decimal? FundingRate { get; set; }
        /// <summary>
        /// Next funding time
        /// </summary>
        public DateTime? NextFundingTime { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFuturesTicker(
            SharedSymbol? sharedSymbol, 
            string symbol, 
            decimal? lastPrice, 
            decimal? highPrice,
            decimal? lowPrice, 
            SharedOrderQuantity volumes, 
            decimal? changePercentage)
            :base(sharedSymbol, symbol)
        {
            LastPrice = lastPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Volumes = volumes;
            ChangePercentage = changePercentage;
        }
    }
}
