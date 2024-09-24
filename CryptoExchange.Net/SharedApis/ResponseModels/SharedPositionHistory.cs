using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Position history
    /// </summary>
    public record SharedPositionHistory
    {
        /// <summary>
        /// Symbol of the position
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// The side of the position
        /// </summary>
        public SharedPositionSide PositionSide { get; set; }
        /// <summary>
        /// Average open price
        /// </summary>
        public decimal AverageOpenPrice { get; set; }
        /// <summary>
        /// Average close price
        /// </summary>
        public decimal AverageClosePrice { get; set; }
        /// <summary>
        /// Position size
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Realized profit/loss
        /// </summary>
        public decimal RealizedPnl { get; set; }
        /// <summary>
        /// Timestamp the position was closed
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Position id
        /// </summary>
        public string? PositionId { get; set; }
        /// <summary>
        /// Leverage of the position
        /// </summary>
        public decimal? Leverage { get; set; }
        /// <summary>
        /// Id of the order that closed the position
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedPositionHistory(
            string symbol,
            SharedPositionSide side,
            decimal openPrice,
            decimal closePrice,
            decimal quantity,
            decimal realizedPnl,
            DateTime timestamp)
        {
            Symbol = symbol;
            PositionSide = side;
            AverageOpenPrice = openPrice;
            AverageClosePrice = closePrice;
            Quantity = quantity;
            RealizedPnl = realizedPnl;
            Timestamp = timestamp;
        }
    }
}
