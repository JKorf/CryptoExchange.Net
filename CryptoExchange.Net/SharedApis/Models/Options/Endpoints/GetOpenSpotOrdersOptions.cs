using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by id endpoint
    /// </summary>
    public class GetOpenSpotOrdersOptions : CapabilityOptions<GetOpenOrdersRequest, IGetOpenSpotOrdersRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve open spot orders";

        /// <summary>
        /// ctor
        /// </summary>
        public GetOpenSpotOrdersOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetOpenSpotOrdersRest.GetOpenSpotOrdersAsync))
        {
        }
    }
}
