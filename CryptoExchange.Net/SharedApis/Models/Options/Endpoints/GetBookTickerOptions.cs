using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting book ticker
    /// </summary>
    public class GetBookTickerOptions : EndpointOptions<GetBookTickerRequest, IBookTickerRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetBookTickerOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IBookTickerRestClient.GetBookTickerAsync))
        {
        }
    }
}
