using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedLeverage
    {
        public decimal Leverage { get; set; }
        public SharedPositionSide? Side { get; set; }
        public SharedMarginMode? MarginMode { get; set; }

        public SharedLeverage(decimal leverage)
        {
            Leverage = leverage;
        }
    }
}
