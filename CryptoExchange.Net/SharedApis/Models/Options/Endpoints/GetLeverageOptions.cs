using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting asset info
    /// </summary>
    public class GetLeverageOptions : EndpointOptions<GetLeverageRequest, IGetLeverageEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the current leverage for a futures symbol";

        /// <summary>
        /// ctor
        /// </summary>
        public GetLeverageOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetLeverageEndpoint.GetLeverageAsync))
        {
        }
    }
}
