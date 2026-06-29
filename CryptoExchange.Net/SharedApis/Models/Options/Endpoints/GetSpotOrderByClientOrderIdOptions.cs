using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by client order id endpoint
    /// </summary>
    public class GetSpotOrderByClientOrderIdOptions : EndpointOptions<GetOrderRequest, ISpotOrderClientIdRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotOrderClientIdRestClient.GetSpotOrderByClientOrderIdAsync))
        {
        }
    }
}
