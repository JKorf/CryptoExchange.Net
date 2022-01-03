﻿using System;

namespace CryptoExchange.Net.ComonObjects
{
    /// <summary>
    /// Order data
    /// </summary>
    public class Order: BaseComonObject
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
        public OrderStatus Status { get; set; }
        /// <summary>
        /// Side of the order
        /// </summary>
        public OrderSide Side { get; set; }
        /// <summary>
        /// Type of the order
        /// </summary>
        public OrderType Type { get; set; }
        /// <summary>
        /// Order time
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}