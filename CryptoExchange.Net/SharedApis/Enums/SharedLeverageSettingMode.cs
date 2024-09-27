namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Leverage setting mode
    /// </summary>
    public enum SharedLeverageSettingMode
    {
        /// <summary>
        /// Leverage is configured per side (in hedge mode)
        /// </summary>
        PerSide,
        /// <summary>
        /// Leverage is configured for the symbol
        /// </summary>
        PerSymbol
    }
}
