namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Type of an order
    /// </summary>
    public enum SharedOrderType
    {
        /// <summary>
        /// Limit order, execute at a specific price
        /// </summary>
        Limit,
        /// <summary>
        /// Limit maker order, a limit order with the condition that is will never be executed as a maker
        /// </summary>
        LimitMaker,
        /// <summary>
        /// Market order, execute at the best price currently available
        /// </summary>
        Market,
        /// <summary>
        /// Other order type, used for parsing unsupported order types
        /// </summary>
        Other
    }
}
