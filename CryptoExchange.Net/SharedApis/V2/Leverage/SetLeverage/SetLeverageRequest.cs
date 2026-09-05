namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to change the current leverage
    /// </summary>
    public record SetLeverageRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Leverage to change to
        /// </summary>
        public decimal Leverage { get; set; }
        /// <summary>
        /// Position side to change leverage for. Some exchanges set leverage per side
        /// </summary>
        public SharedPositionSide? Side { get; set; }
        /// <summary>
        /// Margin mode of the position leverage to change
        /// </summary>
        public SharedMarginMode? MarginMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to change the leverage for</param>
        /// <param name="leverage">Leverage to change to</param>
        /// <param name="positionSide">Position side to change leverage for. Some exchanges set leverage per side</param>
        /// <param name="marginMode">Margin mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SetLeverageRequest(SharedSymbol symbol, decimal leverage, SharedPositionSide? positionSide = null, SharedMarginMode? marginMode = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            Leverage = leverage;
            Side = positionSide;
            MarginMode = marginMode;
        }
    }
}
