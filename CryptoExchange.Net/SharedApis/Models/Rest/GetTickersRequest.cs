
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve all symbol tickers
    /// </summary>
    public record GetTickersRequest : SharedRequest
    {
        /// <summary>
        /// Trading mode
        /// </summary>
        public TradingMode? TradingMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetTickersRequest(TradingMode? tradingMode = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            TradingMode = tradingMode;
        }
    }
}
