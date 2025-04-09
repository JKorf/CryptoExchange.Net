namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Quote asset order quantity support
    /// </summary>
    public enum SharedQuantityType
    {
        /// <summary>
        /// Quantity should be in the base asset
        /// </summary>
        BaseAsset,
        /// <summary>
        /// Quantity should be in the quote asset
        /// </summary>
        QuoteAsset,
        /// <summary>
        /// Quantity is in the number of contracts
        /// </summary>
        Contracts,
        /// <summary>
        /// Quantity can be either base or quote quantity
        /// </summary>
        BaseAndQuoteAsset
    }
}
