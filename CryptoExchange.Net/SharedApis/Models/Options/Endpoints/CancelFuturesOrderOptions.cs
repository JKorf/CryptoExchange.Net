using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a futures order
    /// </summary>
    public class CancelFuturesOrderOptions : EndpointOptions<CancelOrderRequest, IFuturesOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesOrderRestClient.CancelFuturesOrderAsync))
        {
        }
    }
}
