using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Enums
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
