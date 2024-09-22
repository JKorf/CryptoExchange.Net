using CryptoExchange.Net.SharedApis.Enums;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    /// <summary>
    /// Leverage info
    /// </summary>
    public record SharedLeverage
    {
        /// <summary>
        /// Leverage value
        /// </summary>
        public decimal Leverage { get; set; }
        /// <summary>
        /// Side for the leverage
        /// </summary>
        public SharedPositionSide? Side { get; set; }
        /// <summary>
        /// Margin mode
        /// </summary>
        public SharedMarginMode? MarginMode { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedLeverage(decimal leverage)
        {
            Leverage = leverage;
        }
    }
}
