using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a spot order
    /// </summary>
    public class CancelSpotOrderOptions : EndpointOptions<CancelOrderRequest, ISpotOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotOrderRestClient.CancelSpotOrderAsync))
        {
        }
    }
}
