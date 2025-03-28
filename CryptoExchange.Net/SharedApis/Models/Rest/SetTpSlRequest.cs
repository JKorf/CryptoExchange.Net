using CryptoExchange.Net.Objects;

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

        public decimal? TakeProfitTriggerPrice { get; set; }
        public decimal? TakeProfitOrderPrice { get; set; }
        public decimal? StopLossTriggerPrice { get; set; }
        public decimal? StopLossOrderPrice { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SetTpSlRequest(SharedSymbol symbol, SharedPositionSide positionSide, ExchangeParameters? exchangeParameters = null)
            : base(symbol, exchangeParameters)
        {
            PositionSide = positionSide;
            Symbol = symbol;
        }
    }
}
