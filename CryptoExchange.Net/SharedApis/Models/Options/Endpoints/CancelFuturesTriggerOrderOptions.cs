using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling spot trigger order
    /// </summary>
    public class CancelFuturesTriggerOrderOptions : EndpointOptions<CancelOrderRequest, IFuturesTriggerOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesTriggerOrderRestClient.CancelFuturesTriggerOrderAsync))
        {
        }
    }
}
