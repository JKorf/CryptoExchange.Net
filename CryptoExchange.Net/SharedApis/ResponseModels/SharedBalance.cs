namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Balance info
    /// </summary>
    public record SharedBalance
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// Available quantity
        /// </summary>
        public decimal Available { get; set; }
        /// <summary>
        /// Total quantity
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// Isolated margin symbol, only applicable for isolated margin futures
        /// </summary>
        public string? IsolatedMarginSymbol { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedBalance(string asset, decimal available, decimal total)
        {
            Asset = asset;
            Available = available;
            Total = total;
        }
    }
}
