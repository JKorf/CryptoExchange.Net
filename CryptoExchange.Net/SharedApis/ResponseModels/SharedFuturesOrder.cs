using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Futures order info
    /// </summary>
    public record SharedFuturesOrder
    {
        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Id of the order
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// Type of the order
        /// </summary>
        public SharedOrderType OrderType { get; set; }
        /// <summary>
        /// Side of the order
        /// </summary>
        public SharedOrderSide Side { get; set; }
        /// <summary>
        /// Status of the order
        /// </summary>
        public SharedOrderStatus Status { get; set; }
        /// <summary>
        /// Time in force for the order
        /// </summary>
        public SharedTimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Position side
        /// </summary>
        public SharedPositionSide? PositionSide { get; set; }
        /// <summary>
        /// Reduce only
        /// </summary>
        public bool? ReduceOnly { get; set; }
        /// <summary>
        /// Order quantity in the base asset or number of contracts
        /// </summary>
        public decimal? Quantity { get; set; }
        /// <summary>
        /// Quantity filled in the base asset or number of contracts
        /// </summary>
        public decimal? QuantityFilled { get; set; }
        /// <summary>
        /// Quantity of the order in quote asset
        /// </summary>
        public decimal? QuoteQuantity { get; set; }
        /// <summary>
        /// Quantity filled in the quote asset
        /// </summary>
        public decimal? QuoteQuantityFilled { get; set; }
        /// <summary>
        /// Order price
        /// </summary>
        public decimal? OrderPrice { get; set; }
        /// <summary>
        /// Average price
        /// </summary>
        public decimal? AveragePrice { get; set; }
        /// <summary>
        /// Client order id
        /// </summary>
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// Asset the fee is in
        /// </summary>
        public string? FeeAsset { get; set; }
        /// <summary>
        /// Fee paid
        /// </summary>
        public decimal? Fee { get; set; }
        /// <summary>
        /// Leverage
        /// </summary>
        public decimal? Leverage { get; set; }
        /// <summary>
        /// Timestamp the order was created
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// Last trade info, only available for websocket order updates if the API provides this data in the update
        /// </summary>
        public SharedUserTrade? LastTrade { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFuturesOrder(
            string symbol,
            string orderId,
            SharedOrderType orderType,
            SharedOrderSide orderSide,
            SharedOrderStatus orderStatus,
            DateTime? createTime)
        {
            Symbol = symbol;
            OrderId = orderId;
            OrderType = orderType;
            Side = orderSide;
            Status = orderStatus;
            CreateTime = createTime;
        }
    }
}
