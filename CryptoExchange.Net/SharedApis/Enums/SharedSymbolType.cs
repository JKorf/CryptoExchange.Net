namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Type of a symbol
    /// </summary>
    public enum SharedSymbolType
    {
        /// <summary>
        /// Perpetual linear, contract has no delivery date and is settled in stablecoin
        /// </summary>
        PerpetualLinear,
        /// <summary>
        /// Perpetual inverse, contract has no delivery date and is settled in crypto
        /// </summary>
        PerpetualInverse,
        /// <summary>
        /// Delivery linear, contract has a specific delivery date and is settled in stablecoin
        /// </summary>
        DeliveryLinear,
        /// <summary>
        /// Delivery inverse, contract has a specific delivery date and is settled in crypto
        /// </summary>
        DeliveryInverse
    }
}
