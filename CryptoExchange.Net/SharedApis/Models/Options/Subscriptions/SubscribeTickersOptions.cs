using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to ticker updates
    /// </summary>
    public class SubscribeTickersOptions : EndpointOptions<SubscribeAllTickersRequest>
    {
        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTickersOptions(SharedTickerType? tickerCalcType = null) : base(false)
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }
    }
}
