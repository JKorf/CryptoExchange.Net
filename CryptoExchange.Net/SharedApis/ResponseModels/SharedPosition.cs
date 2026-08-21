using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Position info
    /// </summary>
    [DebuggerDisplay("{Symbol,nq} {PositionSide}: {PositionSize} {AverageOpenPrice}")]
    public record SharedPosition : SharedSymbolModel
    {
        /// <summary>
        /// Position id
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// Current size of the position
        /// </summary>
        [Obsolete("Use `PositionSizes` instead")]
        public decimal PositionSize => PositionSizes.QuantityInContracts ?? PositionSizes.QuantityInBaseAsset ?? 0;
        /// <summary>
        /// Current size of the position
        /// </summary>
        public SharedOrderQuantity PositionSizes { get; set; }
        /// <summary>
        /// Side of the position
        /// </summary>
        public SharedPositionSide PositionSide { get; set; }
        /// <summary>
        /// Whether the position is one way mode
        /// </summary>
        public SharedPositionMode PositionMode { get; set; }
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
        /// Stop loss price for the position. Not available in all API's so might be empty even though stop loss price is set
        /// </summary>
        public decimal? StopLossPrice { get; set; }
        /// <summary>
        /// Take profit price for the position. Not available in all API's so might be empty even though stop loss price is set
        /// </summary>
        public decimal? TakeProfitPrice { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedPosition(SharedSymbol? sharedSymbol, string symbol, SharedOrderQuantity positionSize, DateTime? updateTime)
            : base(sharedSymbol, symbol)
        {
            PositionSizes = positionSize;
            UpdateTime = updateTime;
        }
    }
}
