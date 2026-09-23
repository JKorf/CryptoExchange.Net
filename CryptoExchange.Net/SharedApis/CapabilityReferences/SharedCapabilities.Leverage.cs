using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public static partial class SharedCapabilities
    {
        /// <summary>
        /// Leverage capabilities.
        /// </summary>
        public static class Leverage
        {
            /// <summary>
            /// Get leverage capability.
            /// </summary>
            public static SharedRestCapability<IGetLeverage, IGetLeverageRest> GetLeverage { get; } = new();

            /// <summary>
            /// Set leverage capability.
            /// </summary>
            public static SharedRestCapability<ISetLeverage, ISetLeverageRest> SetLeverage { get; } = new();

            /// <summary>
            /// Get leverage tiers capability.
            /// </summary>
            public static SharedRestCapability<IGetLeverageTiers, IGetLeverageTiersRest> GetLeverageTiers { get; } = new();
        }
    }
}
