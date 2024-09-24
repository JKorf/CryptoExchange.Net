namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Position mode selection type
    /// </summary>
    public enum SharedPositionModeSelection
    {
        /// <summary>
        /// Position mode is configured per symbol
        /// </summary>
        PerSymbol,
        /// <summary>
        /// Position mode is configured for the entire account
        /// </summary>
        PerAccount
    }
}
