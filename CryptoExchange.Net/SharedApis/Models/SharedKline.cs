using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Kline info
    /// </summary>
    [DebuggerDisplay("[{OpenTime}] O: {OpenPrice} H: {HighPrice} L: {LowPrice} C: {ClosePrice} V: {Volumes}")]
    public record SharedKline : SharedSymbolModel
    {
        /// <summary>
        /// Open time
        /// </summary>
        public DateTime OpenTime { get; set; }
        /// <summary>
        /// Close price
        /// </summary>
        public decimal ClosePrice { get; set; }
        /// <summary>
        /// High price
        /// </summary>
        public decimal HighPrice { get; set; }
        /// <summary>
        /// Low price
        /// </summary>
        public decimal LowPrice { get; set; }
        /// <summary>
        /// Open price
        /// </summary>
        public decimal OpenPrice { get; set; }

        private decimal? _volume;
        /// <summary>
        /// Volume in the base asset
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
        /// The volume in the last 24h
        /// </summary>
        public SharedOrderQuantity Volumes { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedKline(
            SharedSymbol? sharedSymbol,
            string symbol,
            DateTime openTime, 
            decimal closePrice, 
            decimal highPrice,
            decimal lowPrice,
            decimal openPrice,
            SharedOrderQuantity volumes)
            : base(sharedSymbol, symbol)
        {
            OpenTime = openTime;
            ClosePrice = closePrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            OpenPrice = openPrice;
            Volumes = volumes;
        }
    }
}
