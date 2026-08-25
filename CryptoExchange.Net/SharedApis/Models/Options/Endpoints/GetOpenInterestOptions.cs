using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting open interest
    /// </summary>
    public class GetOpenInterestOptions : EndpointOptions<GetOpenInterestRequest, IGetOpenInterestEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve open interest for a futures symbol";

        /// <summary>
        /// ctor
        /// </summary>
        public GetOpenInterestOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetOpenInterestEndpoint.GetOpenInterestAsync))
        {
        }
    }
}
