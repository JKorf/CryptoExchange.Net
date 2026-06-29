using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a TP/SL
    /// </summary>
    public class CancelFuturesTpSlOptions : EndpointOptions<CancelTpSlRequest, IFuturesTpSlRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesTpSlOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesTpSlRestClient.CancelFuturesTpSlAsync))
        {
        }
    }
}
