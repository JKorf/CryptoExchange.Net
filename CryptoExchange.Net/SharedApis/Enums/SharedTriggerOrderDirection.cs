using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// The order direction when order trigger parameters are reached
    /// </summary>
    public enum SharedTriggerOrderDirection
    {
        /// <summary>
        /// Enter, Buy for Spot and long futures positions, Sell for short futures positions
        /// </summary>
        Enter,
        /// <summary>
        /// Exit, Sell for Spot and long futures positions, Buy for short futures positions
        /// </summary>
        Exit
    }
}
