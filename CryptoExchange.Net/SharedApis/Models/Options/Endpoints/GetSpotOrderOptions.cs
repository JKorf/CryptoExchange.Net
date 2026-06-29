using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by id endpoint
    /// </summary>
    public class GetSpotOrderOptions : EndpointOptions<GetOrderRequest, ISpotOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotOrderRestClient.GetSpotOrderAsync))
        {
        }
    }
}
