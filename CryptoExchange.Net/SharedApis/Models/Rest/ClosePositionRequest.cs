namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to close a currently open position
    /// </summary>
    public record ClosePositionRequest : SharedSymbolRequest
    {
        /// <summary>
        /// The current position mode of the account for the symbol
        /// </summary>
        public SharedPositionMode PositionMode { get; set; }
        /// <summary>
        /// The position side to close. Required when in hedge mode
        /// </summary>
        public SharedPositionSide? PositionSide { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        public SharedMarginMode? MarginMode { get; set; }
        /// <summary>
        /// Quantity of the position to close. Note that the quantity is needed for some exchanges, but will not be respected on other exchanges; don't use it as partial close quantity
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to close the position on</param>
        /// <param name="mode">The current position mode of the account for the symbol</param>
        /// <param name="positionSide">The position side to close. Required when in hedge mode</param>
        /// <param name="marginMode">Margin mode</param>
        /// <param name="quantity">Quantity of the position to close. Note that the quantity is needed for some exchanges, but will not be respected on all exchanges</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public ClosePositionRequest(
            SharedSymbol symbol,
            SharedPositionMode mode,
            SharedPositionSide? positionSide = null,
            SharedMarginMode? marginMode = null,
            decimal? quantity = null,
            ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            PositionMode = mode;
            PositionSide = positionSide;
            MarginMode = marginMode;
            Quantity = quantity;
        }
    }
}
