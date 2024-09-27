namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Trading mode
    /// </summary>
    public enum TradingMode
    {
        /// <summary>
        /// Spot trading
        /// </summary>
        Spot,
        /// <summary>
        /// Perpetual linear futures
        /// </summary>
        PerpetualLinear,
        /// <summary>
        /// Delivery linear futures
        /// </summary>
        DeliveryLinear,
        /// <summary>
        /// Perpetual inverse futures
        /// </summary>
        PerpetualInverse,
        /// <summary>
        /// Delivery inverse futures
        /// </summary>
        DeliveryInverse
    }
}
