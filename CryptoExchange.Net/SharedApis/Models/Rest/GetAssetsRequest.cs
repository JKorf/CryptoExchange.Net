using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve a list of supported assets
    /// </summary>
    public record GetAssetsRequest : SharedRequest
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetAssetsRequest(ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
        }
    }
}
