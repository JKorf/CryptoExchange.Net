namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Set a take profit and/or stop loss for an open position
    /// </summary>
    public record SetTpSlRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Position mode
        /// </summary>
        public SharedPositionMode? PositionMode { get; set; }
        /// <summary>
        /// Position side
        /// </summary>
        public SharedPositionSide PositionSide { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        public SharedMarginMode? MarginMode { get; set; }
        /// <summary>
        /// Take profit / Stop loss side
        /// </summary>
        public SharedTpSlSide TpSlSide { get; set; }
        /// <summary>
        /// Quantity to close. Only used for some API's which require a quantity in the order. Most API's will close the full position
        /// </summary>
        public decimal? Quantity { get; set; }
        /// <summary>
        /// Trigger price
        /// </summary>
        public decimal TriggerPrice { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol of the order</param>
        /// <param name="positionSide">Position side</param>
        /// <param name="tpSlSide">Take Profit / Stop Loss side</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SetTpSlRequest(SharedSymbol symbol, SharedPositionSide positionSide, SharedTpSlSide tpSlSide, decimal triggerPrice, ExchangeParameters? exchangeParameters = null)
            : base(symbol, exchangeParameters)
        {
            PositionSide = positionSide;
            TpSlSide = tpSlSide;
            Symbol = symbol;
            TriggerPrice = triggerPrice;
        }
    }
}
