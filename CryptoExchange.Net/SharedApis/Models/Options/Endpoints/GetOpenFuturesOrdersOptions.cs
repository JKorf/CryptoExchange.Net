using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a futures order by id endpoint
    /// </summary>
    public class GetOpenFuturesOrdersOptions : EndpointOptions<GetOpenOrdersRequest, IFuturesOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetOpenFuturesOrdersOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesOrderRestClient.GetOpenFuturesOrdersAsync))
        {
        }
    }
}
