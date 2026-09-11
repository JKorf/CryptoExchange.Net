using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Balance info
    /// </summary>
    [DebuggerDisplay("{Available} {Asset, nq}")]
    public record SharedBalance
    {
        /// <summary>
        /// Trading modes the balance is for
        /// </summary>
        public TradingMode[] TradingModes { get; set; }
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
        public SharedBalance(TradingMode tradingMode, string asset, decimal available, decimal total)
            : this([tradingMode], asset, available, total) { }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedBalance(TradingMode[] tradingMode, string asset, decimal available, decimal total)
        {
            TradingModes = tradingMode;
            Asset = asset;
            Available = available;
            Total = total;
        }
    }
}
