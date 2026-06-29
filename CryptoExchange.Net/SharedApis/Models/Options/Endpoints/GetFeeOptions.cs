using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting trading fee info
    /// </summary>
    public class GetFeeOptions : EndpointOptions<GetFeeRequest, IFeeRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetFeeOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFeeRestClient.GetFeesAsync))
        {
        }
    }
}
