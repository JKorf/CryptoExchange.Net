using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Futures order info
    /// </summary>
    [DebuggerDisplay("{DebugView,nq}")]
    public record SharedFuturesOrder : SharedSymbolModel
    {
        private string DebugView => 
            $"[{CreateTime}] {OrderId} {(PositionSide != null ? $"{PositionSide} " : "")}{Symbol}  - " +
            $"{OrderType} {Side} {OrderQuantity}{(OrderPrice != null ? " @ " + OrderPrice : "")}, " +
            $"{Status}{(QuantityFilled != null && Status != SharedOrderStatus.Canceled ? $" {QuantityFilled}" : "")}{(AveragePrice != null ? " @ " + AveragePrice : "")}";

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
        private decimal? _averagePrice;
        /// <summary>
        /// Average fill price
        /// </summary>
        public decimal? AveragePrice
        {
            get => _averagePrice > 0 ? _averagePrice
                : (QuantityFilled?.QuantityInBaseAsset > 0 && QuantityFilled?.QuantityInQuoteAsset > 0
                    ? QuantityFilled.QuantityInQuoteAsset / QuantityFilled.QuantityInBaseAsset
                    : null);
            set => _averagePrice = value;
        }
        /// <summary>
        /// Client order id
        /// </summary>
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// Asset the fee is in
        /// </summary>
        [Obsolete("FeeAsset on order level is deprecated and will be removed in a futures version, use FeeAsset on trade level instead")]
        public string? FeeAsset { get; set; }
        /// <summary>
        /// Fee paid
        /// </summary>
        [Obsolete("Fee on order level is deprecated and will be removed in a futures version, use Fee on trade level instead")]
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
        // When V1 Shared API's is deprecated, this property should be marked obsolete as this model will only be used for non-websocket updates
        //[Obsolete("Use SharedFuturesOrderUpdate.LastTrade instead, LastTrade is never filled on non websocket updates")]
        public SharedUserTrade? LastTrade { get; set; }

        /// <summary>
        /// Trigger price for a trigger order
        /// </summary>
        public decimal? TriggerPrice { get; set; }
        /// <summary>
        /// Whether or not the is order is a trigger order
        /// </summary>
        public bool? IsTriggerOrder { get; set; }

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
        public bool? IsCloseOrder { get; set; }

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
