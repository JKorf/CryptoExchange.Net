using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting assets info
    /// </summary>
    public class GetAllAssetsOptions : CapabilityOptions<GetAssetsRequest, IGetAllAssetsRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve basic info for all assets and the networks they support for withdrawals/deposits";

        private static readonly RequestParameterDescription[] _defaultParameterRules = [];

        /// <summary>
        /// ctor
        /// </summary>
        public GetAllAssetsOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetAllAssetsRest.GetAllAssetsAsync), _defaultParameterRules)
        {
        }
    }
}
