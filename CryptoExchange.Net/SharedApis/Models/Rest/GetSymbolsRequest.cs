namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve symbol info
    /// </summary>
    public record GetSymbolsRequest : SharedRequest
    {
        /// <summary>
        /// Symbol type filter
        /// </summary>
        public SymbolType? SymbolType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode filter</param>
        /// <param name="symbolType">Symbol type filter</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetSymbolsRequest(TradingMode? tradingMode = null, SymbolType? symbolType = null, ExchangeParameters? exchangeParameters = null) : base(tradingMode, exchangeParameters)
        {
            SymbolType = symbolType;
        }
    }
}
