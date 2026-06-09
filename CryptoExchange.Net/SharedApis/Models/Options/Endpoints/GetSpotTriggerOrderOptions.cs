using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting spot trigger order
    /// </summary>
    public class GetSpotTriggerOrderOptions : EndpointOptions<GetOrderRequest, ISpotTriggerOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotTriggerOrderRestClient.GetSpotTriggerOrderAsync))
        {
        }
    }
}
