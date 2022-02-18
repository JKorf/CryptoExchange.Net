namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Position data
    /// </summary>
    public class Position: BaseCommonObject
    {
        /// <summary>
        /// Id of the position
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// Symbol of the position
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Leverage
        /// </summary>
        public decimal Leverage { get; set; }
        /// <summary>
        /// Position quantity
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Entry price
        /// </summary>
        public decimal? EntryPrice { get; set; }
        /// <summary>
        /// Liquidation price
        /// </summary>
        public decimal? LiquidationPrice { get; set; }
        /// <summary>
        /// Unrealized profit and loss
        /// </summary>
        public decimal? UnrealizedPnl { get; set; }
        /// <summary>
        /// Realized profit and loss
        /// </summary>
        public decimal? RealizedPnl { get; set; }
        /// <summary>
        /// Mark price
        /// </summary>
        public decimal? MarkPrice { get; set; }
        /// <summary>
        /// Auto adding margin
        /// </summary>
        public bool? AutoMargin { get; set; }
        /// <summary>
        /// Position margin
        /// </summary>
        public decimal? PositionMargin { get; set; }
        /// <summary>
        /// Position side
        /// </summary>
        public CommonPositionSide? Side { get; set; }
        /// <summary>
        /// Is isolated
        /// </summary>
        public bool? Isolated { get; set; }
        /// <summary>
        /// Maintenance margin
        /// </summary>
        public decimal? MaintananceMargin { get; set; }
    }
}
