using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for closing position
    /// </summary>
    public class ClosePositionOptions : EndpointOptions<ClosePositionRequest, IFuturesOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ClosePositionOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesOrderRestClient.ClosePositionAsync))
        {
        }
    }
}
