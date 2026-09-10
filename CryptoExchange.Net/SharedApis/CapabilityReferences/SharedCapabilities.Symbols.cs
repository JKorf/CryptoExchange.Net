using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Symbol capabilities.
        /// </summary>
        public static class Symbols
        {
            /// <summary>
            /// Get spot symbols capability.
            /// </summary>
            public static SharedRestCapability<IGetSpotSymbols, IGetSpotSymbolsRest> GetSpotSymbols { get; } = new();

            /// <summary>
            /// Get futures symbols capability.
            /// </summary>
            public static SharedRestCapability<IGetFuturesSymbols, IGetFuturesSymbolsRest> GetFuturesSymbols { get; } = new();
        }
    }
}
