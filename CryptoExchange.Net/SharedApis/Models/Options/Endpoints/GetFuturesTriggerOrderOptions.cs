using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting futures trigger order
    /// </summary>
    public class GetFuturesTriggerOrderOptions : EndpointOptions<GetOrderRequest, IFuturesTriggerOrderRestClient>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a futures trigger order";

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesTriggerOrderRestClient.GetFuturesTriggerOrderAsync))
        {
        }
    }
}
