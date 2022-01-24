using System;

namespace CryptoExchange.Net.CommonObjects
{
    /// <summary>
    /// Order data
    /// </summary>
    public class Order: BaseCommonObject
    {
        /// <summary>
        /// Id of the order
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Symbol of the order
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Price of the order
        /// </summary>
        public decimal? Price { get; set; }
        /// <summary>
        /// Quantity of the order
        /// </summary>
        public decimal? Quantity { get; set; }
        /// <summary>
        /// The quantity of the order which has been filled
        /// </summary>
        public decimal? QuantityFilled { get; set; }
        /// <summary>
        /// Status of the order
        /// </summary>
        public CommonOrderStatus Status { get; set; }
        /// <summary>
        /// Side of the order
        /// </summary>
        public CommonOrderSide Side { get; set; }
        /// <summary>
        /// Type of the order
        /// </summary>
        public CommonOrderType Type { get; set; }
        /// <summary>
        /// Order time
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
