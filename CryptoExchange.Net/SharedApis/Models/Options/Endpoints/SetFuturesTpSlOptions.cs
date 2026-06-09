using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting a TP/SL
    /// </summary>
    public class SetFuturesTpSlOptions : EndpointOptions<SetTpSlRequest, IFuturesTpSlRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SetFuturesTpSlOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesTpSlRestClient.SetFuturesTpSlAsync))
        {
        }
    }
}
