namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Position mode
    /// </summary>
    public enum SharedPositionMode
    {
        /// <summary>
        /// Hedge mode, a symbol can have both a long and a short position at the same time
        /// </summary>
        HedgeMode,
        /// <summary>
        /// One way mode, a symbol can only have one open position side at a time
        /// </summary>
        OneWay
    }
}
