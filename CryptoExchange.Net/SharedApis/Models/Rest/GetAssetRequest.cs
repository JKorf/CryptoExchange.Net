namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve info on a specific asset
    /// </summary>
    public record GetAssetRequest : SharedRequest
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string Asset { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="asset">Asset to retrieve info on</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetAssetRequest(string asset, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Asset = asset;
        }
    }
}
