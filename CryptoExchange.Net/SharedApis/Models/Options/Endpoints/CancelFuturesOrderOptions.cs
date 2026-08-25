using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a futures order
    /// </summary>
    public class CancelFuturesOrderOptions : EndpointOptions<CancelOrderRequest, ICancelFuturesOrderEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a futures order";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelFuturesOrderEndpoint.CancelFuturesOrderAsync))
        {
        }
    }
}
