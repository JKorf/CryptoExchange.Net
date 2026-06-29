using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting open positions
    /// </summary>
    public class GetPositionsOptions : EndpointOptions<GetPositionsRequest, IFuturesOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesOrderRestClient.GetPositionsAsync))
        {
        }
    }
}
