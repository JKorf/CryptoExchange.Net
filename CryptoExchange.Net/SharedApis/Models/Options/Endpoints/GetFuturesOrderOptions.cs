using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a futures order by id endpoint
    /// </summary>
    public class GetFuturesOrderOptions : EndpointOptions<GetOrderRequest, IGetFuturesOrderEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a futures order";

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetFuturesOrderEndpoint.GetFuturesOrderAsync))
        {
        }
    }
}
