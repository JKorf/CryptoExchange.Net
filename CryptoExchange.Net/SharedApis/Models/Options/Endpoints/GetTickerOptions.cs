using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting ticker
    /// </summary>
    public class GetTickerOptions : EndpointOptions<GetTickerRequest>
    {
        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetTickerOptions(SharedTickerType? tickerCalcType = null) : base(false)
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Ticker time calc type: {TickerType}");
            return sb.ToString();
        }
    }
}
