using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting deposit address
    /// </summary>
    public class GetDepositAddressOptions : EndpointOptions<GetDepositAddressesRequest, IDepositRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetDepositAddressOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IDepositRestClient.GetDepositAddressesAsync))
        {
        }
    }
}
