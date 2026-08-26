using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a trades for an order
    /// </summary>
    public class GetFuturesOrderTradesOptions : CapabilityOptions<GetOrderTradesRequest, IGetFuturesOrderTradesEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve trades for a futures order";

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesOrderTradesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetFuturesOrderTradesEndpoint.GetFuturesOrderTradesAsync))
        {
        }
    }
}
