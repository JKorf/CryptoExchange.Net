namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to place a new futures order
    /// </summary>
    public record PlaceFuturesOrderRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Side of the order
        /// </summary>
        public SharedOrderSide Side { get; set; }
        /// <summary>
        /// Type of the order
        /// </summary>
        public SharedOrderType OrderType { get; set; }
        /// <summary>
        /// Time in force of the order
        /// </summary>
        public SharedTimeInForce? TimeInForce { get; set; }
        /// <summary>
        /// Quantity of the order in base asset.
        /// </summary>
        public decimal? Quantity { get; set; }
        /// <summary>
        /// Quantity of the order in quote asset.
        /// </summary>
        public decimal? QuoteQuantity { get; set; }
        /// <summary>
        /// Price of the order
        /// </summary>
        public decimal? Price { get; set; }
        /// <summary>
        /// Client order id
        /// </summary>
        public string? ClientOrderId { get; set; }
        /// <summary>
        /// Position side of the order. Required when in hedge mode, ignored in one-way mode
        /// </summary>
        public SharedPositionSide? PositionSide { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        public SharedMarginMode? MarginMode { get; set; }
        /// <summary>
        /// Reduce only order
        /// </summary>
        public bool? ReduceOnly { get; set; }
        /// <summary>
        /// Leverage for the position. Note that leverage might not be applied during order placement but instead needs to be set before opening the position depending on the exchange. In this case use the SetLeverageAsync method.
        /// </summary>
        public decimal? Leverage { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to place the order on</param>

        /// <param name="side">Side of the order</param>
        /// <param name="orderType">Type of the order</param>
        /// <param name="quantity">Quantity of the order</param>
        /// <param name="quoteQuantity">Quantity of the order in quote asset</param>
        /// <param name="price">Price of the order</param>
        /// <param name="timeInForce">Time in force</param>
        /// <param name="clientOrderId">Client order id</param>
        /// <param name="reduceOnly">Reduce only</param>
        /// <param name="leverage">Leverage for the position. Note that leverage might not be applied during order placement but instead needs to be set before opening the position depending on the exchange. In this case use the SetLeverageAsync method.</param>
        /// <param name="positionSide">Position side of the order. Required when in hedge mode, ignored in one-way mode</param>
        /// <param name="marginMode">Margin mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public PlaceFuturesOrderRequest(
            SharedSymbol symbol,
            SharedOrderSide side,
            SharedOrderType orderType,
            decimal? quantity = null,
            decimal? quoteQuantity = null,
            decimal? price = null,
            bool? reduceOnly = null,
            decimal? leverage = null,
            SharedTimeInForce? timeInForce = null,
            SharedPositionSide? positionSide = null,
            SharedMarginMode? marginMode = null,
            string? clientOrderId = null,
            ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            Side = side;
            OrderType = orderType;
            Quantity = quantity;
            QuoteQuantity = quoteQuantity;
            Price = price;
            MarginMode = marginMode;
            ClientOrderId = clientOrderId;
            ReduceOnly = reduceOnly;
            Leverage = leverage;
            TimeInForce = timeInForce;
            PositionSide = positionSide;
        }
    }
}
