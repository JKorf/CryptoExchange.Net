using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve open positions
    /// </summary>
    public record GetPositionsRequest : SharedRequest
    {
        /// <summary>
        /// Trading mode
        /// </summary>
        public TradingMode? TradingMode { get; set; }
        /// <summary>
        /// Symbol filter, required for some exchanges
        /// </summary>
        public SharedSymbol? Symbol { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetPositionsRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            TradingMode = tradingMode;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retriecve positions for</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetPositionsRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters) 
        {
            Symbol = symbol;
        }
    }
}
