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
        public SharedAssetType? BaseAssetType { get; }
        /// <summary>
        /// Symbol subtype filter
        /// </summary>
        public SharedAssetSubType? BaseAssetSubType { get; }
        /// <summary>
        /// Symbol type filter
        /// </summary>
        public SharedAssetType? QuoteAssetType { get; }
        /// <summary>
        /// Symbol subtype filter
        /// </summary>
        public SharedAssetSubType? QuoteAssetSubType { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradingMode">Trading mode filter</param>
        /// <param name="baseAssetType">Filter by base asset type</param>
        /// <param name="baseAssetSubType">Filter by base asset subtype</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetSymbolsRequest(
            TradingMode? tradingMode = null,
            SharedAssetType? baseAssetType = null, 
            SharedAssetSubType? baseAssetSubType = null,
            SharedAssetType? quoteAssetType = null,
            SharedAssetSubType? quoteAssetSubType = null,
            ExchangeParameters? exchangeParameters = null) : base(tradingMode, exchangeParameters)
        {
            BaseAssetType = baseAssetType;
            BaseAssetSubType = baseAssetSubType;
            QuoteAssetType = quoteAssetType;
            QuoteAssetSubType = quoteAssetSubType;
        }
    }
}
