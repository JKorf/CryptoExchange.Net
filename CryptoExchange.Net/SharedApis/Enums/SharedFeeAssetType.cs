namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Fee asset selection type
    /// </summary>
    public enum SharedFeeAssetType
    {
        /// <summary>
        /// Fee is always in the base asset
        /// </summary>
        BaseAsset,
        /// <summary>
        /// Fee is always in the quote asset
        /// </summary>
        QuoteAsset,
        /// <summary>
        /// Fee is always in the input asset
        /// </summary>
        InputAsset,
        /// <summary>
        /// Fee is always in the output asset
        /// </summary>
        OutputAsset,
        /// <summary>
        /// Fee is variable
        /// </summary>
        Variable
    }
}
