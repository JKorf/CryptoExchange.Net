using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting book ticker
    /// </summary>
    public class GetBookTickerOptions : CapabilityOptions<GetBookTickerRequest, IGetBookTickerRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the best bid and ask price for a symbol";

        /// <summary>
        /// ctor
        /// </summary>
        public GetBookTickerOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetBookTickerRest.GetBookTickerAsync))
        {
        }
    }
}
