namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Status of an order
    /// </summary>
    public enum SharedOrderStatus
    {
        /// <summary>
        /// Order is open waiting to be filled
        /// </summary>
        Open,
        /// <summary>
        /// Order has been fully filled
        /// </summary>
        Filled,
        /// <summary>
        /// Order has been canceled
        /// </summary>
        Canceled
    }
}
