using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a spot order
    /// </summary>
    public class CancelSpotOrderByClientOrderIdOptions : EndpointOptions<CancelOrderRequest, ISpotOrderClientIdRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotOrderClientIdRestClient.CancelSpotOrderByClientOrderIdAsync))
        {
        }
    }
}
