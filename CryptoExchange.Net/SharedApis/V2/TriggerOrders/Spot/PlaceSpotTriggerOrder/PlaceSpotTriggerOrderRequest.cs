namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to place a new trigger order
    /// </summary>
    public record PlaceSpotTriggerOrderRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Client order id
        /// </summary>
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// Direction of the trigger order
        /// </summary>
        public SharedOrderSide OrderSide { get; set; }
        /// <summary>
        /// Price trigger direction
        /// </summary>
        public SharedTriggerPriceDirection PriceDirection { get; set; }
        /// <summary>
        /// Time in force
        /// </summary>
        public SharedTimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Quantity of the order
        /// </summary>
        public SharedQuantity Quantity { get; set; }
        /// <summary>
        /// Price of the order
        /// </summary>
        public decimal? OrderPrice { get; set; }
        /// <summary>
        /// Trigger price
        /// </summary>
        public decimal TriggerPrice { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol the order is on</param>
        /// <param name="orderSide">Order side</param>
        /// <param name="priceDirection">Price direction</param>
        /// <param name="quantity">Quantity of the order</param>
        /// <param name="triggerPrice">Price at which the order should activate</param>
        /// <param name="orderPrice">Limit price for the order</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public PlaceSpotTriggerOrderRequest(SharedSymbol symbol,
            SharedTriggerPriceDirection priceDirection,
            decimal triggerPrice,
            SharedOrderSide orderSide,
            SharedQuantity quantity,
            decimal? orderPrice = null,
            ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            PriceDirection = priceDirection;
            Quantity = quantity;
            OrderPrice = orderPrice;
            TriggerPrice = triggerPrice;
            OrderSide = orderSide;
        }
    }
}
