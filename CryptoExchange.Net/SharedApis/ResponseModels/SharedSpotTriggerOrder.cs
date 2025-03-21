using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Trigger order info
    /// </summary>
    public record SharedSpotTriggerOrder : SharedSymbolModel
    {
        /// <summary>
        /// The id of the order
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// The type of the order
        /// </summary>
        public SharedOrderType OrderType { get; set; }
        /// <summary>
        /// Status of the order
        /// </summary>
        #warning different statuses? For example whether or not triggered
        public SharedOrderStatus Status { get; set; }
        /// <summary>
        /// Time in force for the order
        /// </summary>
        public SharedTimeInForce? TimeInForce { get; set; }
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
        /// Average fill price
        /// </summary>
        public decimal? AveragePrice { get; set; }
        /// <summary>
        /// Trigger order direction
        /// </summary>
        public SharedTriggerOrderDirection OrderDirection { get; set; }
        /// <summary>
        /// Trigger price
        /// </summary>
        public decimal TriggerPrice { get; set; }
        /// <summary>
        /// Asset the fee is in
        /// </summary>
        public string? FeeAsset { get; set; }
        /// <summary>
        /// Fee paid for the order
        /// </summary>
        public decimal? Fee { get; set; }
        /// <summary>
        /// Timestamp the order was created
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime? UpdateTime { get; set; }

#warning Split status into `Triggered` and `OrderStatus`
#warning Split id field in TriggerOrderId and ExecuteOrderId (nullable)
#warning Is Buy/Sell easier to map than enter/exit?

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSpotTriggerOrder(
            SharedSymbol? sharedSymbol,
            string symbol,
            string orderId,
            SharedOrderType orderType,
            SharedTriggerOrderDirection orderDirection,
            SharedOrderStatus orderStatus,
            decimal triggerPrice,
            DateTime? createTime)
            : base(sharedSymbol, symbol)
        {
            OrderId = orderId;
            OrderType = orderType;
            OrderDirection = orderDirection;
            Status = orderStatus;
            CreateTime = createTime;
            TriggerPrice = triggerPrice;
        }
    }
}