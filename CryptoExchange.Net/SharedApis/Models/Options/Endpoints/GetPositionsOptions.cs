using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting open positions
    /// </summary>
    public class GetPositionsOptions : CapabilityOptions<GetPositionsRequest, IGetPositionsEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve open futures positions";

        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetPositionsEndpoint.GetPositionsAsync))
        {
        }
    }
}
