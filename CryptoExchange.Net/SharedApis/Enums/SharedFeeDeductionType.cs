using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Enums
{
    public enum SharedFeeDeductionType
    {
        /// <summary>
        /// The fee is deducted from the output amount. For example buying 1 ETH at 1000 USDT with a 1% fee would cost 1000 USDT and output 0.99 ETH
        /// </summary>
        DeductFromTrade,
        /// <summary>
        /// The fee is added to the order cost. For example buying 1 ETH at 1000 USDT with a 1% fee would cost 1010 USDT and output 1 ETH
        /// </summary>
        AddToCost
    }
}
