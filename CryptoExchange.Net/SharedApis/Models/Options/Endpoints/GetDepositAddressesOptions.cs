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

        /// <summary>
        /// ctor
        /// </summary>
        public GetDepositAddressesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetDepositAddressesRest.GetDepositAddressesAsync))
        {
        }
    }
}
