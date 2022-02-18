using System;

namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Trade data
    /// </summary>
    public class Trade: BaseCommonObject
    {
        /// <summary>
        /// Symbol of the trade
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Price of the trade
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Quantity of the trade
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Timestamp of the trade
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// User trade info
    /// </summary>
    public class UserTrade: Trade
    {
        /// <summary>
        /// Id of the trade
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Order id of the trade
        /// </summary>
        public string? OrderId { get; set; }
        /// <summary>
        /// Fee of the trade
        /// </summary>
        public decimal? Fee { get; set; }
        /// <summary>
        /// The asset the fee is paid in
        /// </summary>
        public string? FeeAsset { get; set; }
    }
}
