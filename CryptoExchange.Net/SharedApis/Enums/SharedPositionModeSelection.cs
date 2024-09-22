using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Enums
{
    /// <summary>
    /// Position mode selection type
    /// </summary>
    public enum SharedPositionModeSelection
    {
        /// <summary>
        /// Position mode is configured per symbol
        /// </summary>
        PerSymbol,
        /// <summary>
        /// Position mode is configured for the entire account
        /// </summary>
        PerAccount
    }
}
