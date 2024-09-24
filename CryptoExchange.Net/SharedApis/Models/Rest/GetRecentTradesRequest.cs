namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the most recent trades of a symbol
    /// </summary>
    public record GetRecentTradesRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Max number of results
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retrieve trades for</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetRecentTradesRequest(SharedSymbol symbol, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            Limit = limit;
        }
    }
}
