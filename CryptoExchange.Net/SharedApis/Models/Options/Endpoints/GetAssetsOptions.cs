using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting assets info
    /// </summary>
    public class GetAssetsOptions : EndpointOptions<GetAssetsRequest, IAssetsRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetAssetsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IAssetsRestClient.GetAssetsAsync))
        {
        }
    }
}
