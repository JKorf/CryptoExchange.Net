using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting spot trigger order
    /// </summary>
    public class CancelSpotTriggerOrderOptions : EndpointOptions<CancelOrderRequest, ISpotTriggerOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotTriggerOrderRestClient.CancelSpotTriggerOrderAsync))
        {
        }
    }
}
