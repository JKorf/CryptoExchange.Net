using CryptoExchange.Net.SharedApis.Enums;
using System;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    /// <summary>
    /// A user trade
    /// </summary>
    public record SharedUserTrade
    {
        /// <summary>
        /// Symbol the trade was on
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// The trade id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Traded quantity
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Trade price
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Trade timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The order id
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// Fee paid for the trade
        /// </summary>
        public decimal? Fee { get; set; }
        /// <summary>
        /// The asset the fee is in
        /// </summary>
        public string? FeeAsset { get; set; }
        /// <summary>
        /// Trade role
        /// </summary>
        public SharedRole? Role { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedUserTrade(string symbol, string orderId, string id, decimal quantity, decimal price, DateTime timestamp)
        {
            Symbol = symbol;
            OrderId = orderId;
            Id = id;
            Quantity = quantity;
            Price = price;
            Timestamp = timestamp;
        }
    }
}
