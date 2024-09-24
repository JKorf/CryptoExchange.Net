using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to change the current position mode 
    /// </summary>
    public record SetPositionModeRequest : SharedRequest
    {
        /// <summary>
        /// Symbol to change the mode for. Depending on the exchange position mode is set for the whole account or per symbol
        /// </summary>
        public SharedSymbol? Symbol { get; set; }
        /// <summary>
        /// Trading mode
        /// </summary>
        public TradingMode? TradingMode { get; set; }
        /// <summary>
        /// Position mode to change to
        /// </summary>
        public SharedPositionMode PositionMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="positionMode">Position mode to change to</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SetPositionModeRequest(SharedPositionMode positionMode, TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            TradingMode = tradingMode;
            PositionMode = positionMode;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to change to position mode for</param>
        /// <param name="positionMode">Position mode to change to</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public SetPositionModeRequest(SharedSymbol symbol, SharedPositionMode positionMode, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            PositionMode = positionMode;
            Symbol = symbol;
        }
    }
}
