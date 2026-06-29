using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting asset info
    /// </summary>
    public class GetLeverageOptions : EndpointOptions<GetLeverageRequest, ILeverageRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetLeverageOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ILeverageRestClient.GetLeverageAsync))
        {
        }
    }
}
