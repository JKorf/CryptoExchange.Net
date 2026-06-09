using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting trades for a spot order
    /// </summary>
    public class GetSpotOrderTradesOptions : EndpointOptions<GetOrderTradesRequest, ISpotOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotOrderTradesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotOrderRestClient.GetSpotOrderTradesAsync))
        {
        }
    }
}
