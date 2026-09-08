namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to fully close a currently open position
    /// </summary>
    public record CloseFullPositionRequest : SharedSymbolRequest
    {
        /// <summary>
        /// The id of the position to close
        /// </summary>
        public string? PositionId { get; set; }

        /// <summary>
        /// The current position mode of the account for the symbol
        /// </summary>
        public SharedPositionMode? PositionMode { get; set; }
        /// <summary>
        /// The position side to close. Required when in hedge mode
        /// </summary>
        public SharedPositionSide? PositionSide { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        public SharedMarginMode? MarginMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to close the position on</param>
        /// <param name="mode">The current position mode of the account for the symbol</param>
        /// <param name="positionSide">The position side to close. Required when in hedge mode</param>
        /// <param name="marginMode">Margin mode</param>
        /// <param name="positionId">Id of the position to close</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public CloseFullPositionRequest(
            SharedSymbol symbol,
            SharedPositionMode? mode = null,
            SharedPositionSide? positionSide = null,
            SharedMarginMode? marginMode = null,
            string? positionId = null,
            ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            PositionId = positionId;
            PositionMode = mode;
            PositionSide = positionSide;
            MarginMode = marginMode;
        }
    }
}
