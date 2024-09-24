namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the leverage setting for a symbol
    /// </summary>
    public record GetLeverageRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Position side, required when in hedge mode
        /// </summary>
        public SharedPositionSide? PositionSide { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        public SharedMarginMode? MarginMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to request leverage for</param>
        /// <param name="positionSide">Position side to get leverage for when in hedge mode</param>
        /// <param name="marginMode">Margin mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetLeverageRequest(SharedSymbol symbol, SharedPositionSide? positionSide = null, SharedMarginMode? marginMode = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            PositionSide = positionSide;
            MarginMode = marginMode;
        }
    }
}
