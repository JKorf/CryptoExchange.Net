using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Futures order info
    /// </summary>
    public record SharedFuturesOrder : SharedSymbolModel
    {
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
        /// Order quantity
        /// </summary>
        public SharedOrderQuantity? OrderQuantity { get; set; }
        /// <summary>
        /// Filled quantity
        /// </summary>
        public SharedOrderQuantity? QuantityFilled { get; set; }
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
        /// Trigger price for a trigger order
        /// </summary>
        public decimal? TriggerPrice { get; set; }
        /// <summary>
        /// Whether or not the is order is a trigger order
        /// </summary>
        public bool IsTriggerOrder { get; set; }

        /// <summary>
        /// Take profit price
        /// </summary>
        public decimal? TakeProfitPrice { get; set; }

        /// <summary>
        /// Stop loss price
        /// </summary>
        public decimal? StopLossPrice { get; set; }

        /// <summary>
        /// Whether this order is to close an existing position. If this is the case quantities might not be specified
        /// </summary>
        public bool IsCloseOrder { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFuturesOrder(
            SharedSymbol? sharedSymbol, 
            string symbol,
            string orderId,
            SharedOrderType orderType,
            SharedOrderSide orderSide,
            SharedOrderStatus orderStatus,
            DateTime? createTime)
            : base(sharedSymbol, symbol)
        {
            OrderId = orderId;
            OrderType = orderType;
            Side = orderSide;
            Status = orderStatus;
            CreateTime = createTime;
        }
    }
}
