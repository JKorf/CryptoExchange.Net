using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Leverage tier
    /// </summary>
    [DebuggerDisplay("[{Index}] {MinNotional} - {MaxNotional}")]
    public record SharedLeverageTier : SharedSymbolModel
    {
        /// <summary>
        /// Tier index
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// The asset the notional value is in
        /// </summary>
        public string? Asset { get; set; }
        /// <summary>
        /// Min notional value for activation of this tier
        /// </summary>
        public decimal MinNotional { get; set; }
        /// <summary>
        /// Max notional value for this tier, null if no limit
        /// </summary>
        public decimal? MaxNotional { get; set; }
        /// <summary>
        /// Maintenance margin rate for this tier
        /// </summary>
        public decimal? MaintenanceMarginRate { get; set; }
        /// <summary>
        /// Max leverage for this tier
        /// </summary>
        public decimal? MaxLeverage { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedLeverageTier(SharedSymbol? sharedSymbol, string symbol, int index, string? asset, decimal minNotional, decimal? maxNotional, decimal? maintenanceMarginRate, decimal? maxLeverage)
            : base(sharedSymbol, symbol)
        {
            Index = index;
            Asset = asset;
            MinNotional = minNotional;
            MaxNotional = maxNotional;
            MaintenanceMarginRate = maintenanceMarginRate;
            MaxLeverage = maxLeverage;
        }

    }
}
