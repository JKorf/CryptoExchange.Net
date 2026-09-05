using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting deposit address
    /// </summary>
    public class GetDepositAddressesOptions : CapabilityOptions<GetDepositAddressesRequest, IGetDepositAddressesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve deposit addresses for an asset";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetDepositAddressesRequest>.Required(x => x.Asset, "The asset to retrieve deposit addresses for", "ETH"),
            RequestParameterRule<GetDepositAddressesRequest>.Optional(x => x.Network, "The network to retrieve a deposit address for", "ERC20"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetDepositAddressesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetDepositAddressesRest.GetDepositAddressesAsync), _defaultParameterRules)
        {
        }
    }
}
