using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Position info
    /// </summary>
    public record SharedPosition
    {
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Current size of the position
        /// </summary>
        public decimal PositionSize { get; set; }
        /// <summary>
        /// Side of the position
        /// </summary>
        public SharedPositionSide PositionSide { get; set; }
        /// <summary>
        /// Average open price
        /// </summary>
        public decimal? AverageOpenPrice { get; set; }
        /// <summary>
        /// Current unrealized profit/loss
        /// </summary>
        public decimal? UnrealizedPnl { get; set; }
        /// <summary>
        /// Liquidation price
        /// </summary>
        public decimal? LiquidationPrice { get; set; }
        /// <summary>
        /// Leverage
        /// </summary>
        public decimal? Leverage { get; set; }
        /// <summary>
        /// Last update time
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedPosition(string symbol, decimal positionSize, DateTime? updateTime)
        {
            Symbol = symbol;
            PositionSize = positionSize;
            UpdateTime = updateTime;
        }
    }
}
