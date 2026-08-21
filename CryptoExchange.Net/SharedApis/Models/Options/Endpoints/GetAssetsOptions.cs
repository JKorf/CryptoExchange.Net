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
        /// <inheritdoc />
        public override string Description => "Retrieve basic info for all assets and the networks they support for withdrawals/deposits";

        /// <summary>
        /// ctor
        /// </summary>
        public GetAssetsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IAssetsRestClient.GetAssetsAsync))
        {
        }
    }
}
