using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Price direction for trigger order
    /// </summary>
    public enum SharedTriggerPriceDirection
    {
        /// <summary>
        /// Trigger when the price goes below the specified trigger price
        /// </summary>
        PriceBelow,
        /// <summary>
        /// Trigger when the price goes above the specified trigger price
        /// </summary>
        PriceAbove
    }
}
