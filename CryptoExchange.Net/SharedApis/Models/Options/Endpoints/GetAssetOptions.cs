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
        /// <inheritdoc />
        public override string Description => "Retrieve basic info for a single asset and networks it supports for withdrawals/deposits";

        /// <summary>
        /// ctor
        /// </summary>
        public GetAssetOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IAssetsRestClient.GetAssetAsync))
        {
        }
    }
}
