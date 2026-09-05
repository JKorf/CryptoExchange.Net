using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting asset info
    /// </summary>
    public class GetAssetOptions : CapabilityOptions<GetAssetRequest, IGetAssetRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve basic info for a single asset and networks it supports for withdrawals/deposits";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetAssetRequest>.Required(x => x.Asset, "The asset to retrieve information for", "ETH"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetAssetOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetAssetRest.GetAssetAsync), _defaultParameterRules)
        {
        }
    }
}
