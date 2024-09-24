using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve symbol info
    /// </summary>
    public record GetSymbolsRequest : SharedRequest
    {
        /// <summary>
        /// Filter by trading mode
        /// </summary>
        public TradingMode? TradingMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode filter</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetSymbolsRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            TradingMode = tradingMode;
        }
    }
}
