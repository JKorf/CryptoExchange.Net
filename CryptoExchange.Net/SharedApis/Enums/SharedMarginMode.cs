namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Margin mode
    /// </summary>
    public enum SharedMarginMode
    {
        /// <summary>
        /// Cross margin, margin is shared across symbols
        /// </summary>
        Cross,
        /// <summary>
        /// Isolated margin, margin is isolated on a symbol
        /// </summary>
        Isolated
    }
}
