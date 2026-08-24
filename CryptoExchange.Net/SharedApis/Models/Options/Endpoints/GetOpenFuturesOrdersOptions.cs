using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a futures order by id endpoint
    /// </summary>
    public class GetOpenFuturesOrdersOptions : EndpointOptions<GetOpenOrdersRequest, IGetOpenFuturesOrdersRestClient>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve open futures orders";

        /// <summary>
        /// ctor
        /// </summary>
        public GetOpenFuturesOrdersOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetOpenFuturesOrdersRestClient.GetOpenFuturesOrdersAsync))
        {
        }
    }
}
