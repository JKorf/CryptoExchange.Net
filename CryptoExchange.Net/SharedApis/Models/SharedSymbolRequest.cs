namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Symbol request
    /// </summary>
    public record SharedSymbolRequest : SharedRequest
    {
        /// <summary>
        /// The symbol
        /// </summary>
        public SharedSymbol Symbol { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSymbolRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Symbol = symbol;
        }
    }
}
