namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the deposit addresses for an asset
    /// </summary>
    public record GetDepositAddressesRequest : SharedRequest
    {
        /// <summary>
        /// Asset to get address for
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// Network name
        /// </summary>
        public string? Network { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="asset">Asset name to get address for</param>
        /// <param name="network">Network name</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetDepositAddressesRequest(string asset, string? network = null, ExchangeParameters? exchangeParameters = null): base(exchangeParameters)
        {
            Asset = asset;
            Network = network;
        }
    }
}
