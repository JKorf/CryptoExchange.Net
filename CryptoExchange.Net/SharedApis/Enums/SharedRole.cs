namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// The role of a trade
    /// </summary>
    public enum SharedRole
    {
        /// <summary>
        /// Maker role, put an order on the order book which has been filled
        /// </summary>
        Maker,
        /// <summary>
        /// Taker role, took an order of the order book to fill
        /// </summary>
        Taker
    }
}
