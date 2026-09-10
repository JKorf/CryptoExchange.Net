using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Mark price capabilities.
        /// </summary>
        public static class MarkPrices
        {
            /// <summary>
            /// Get a mark price capability.
            /// </summary>
            public static SharedRestCapability<IGetMarkPrice, IGetMarkPriceRest> GetMarkPrice { get; } = new();

            /// <summary>
            /// Get all mark prices capability.
            /// </summary>
            public static SharedRestCapability<IGetAllMarkPrices, IGetAllMarkPricesRest> GetAllMarkPrices { get; } = new();

            /// <summary>
            /// Subscribe to mark price updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeMarkPriceSocket> SubscribeMarkPrice { get; } = new();
        }
    }
}
