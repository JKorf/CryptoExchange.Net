using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting asset info
    /// </summary>
    public class GetAssetOptions : EndpointOptions<GetAssetRequest, IAssetsRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetAssetOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IAssetsRestClient.GetAssetAsync))
        {
        }
    }
}
