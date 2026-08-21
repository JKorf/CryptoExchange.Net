using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by client order id
    /// </summary>
    public class GetFuturesOrderByClientOrderIdOptions : EndpointOptions<GetOrderRequest, IFuturesOrderClientIdRestClient>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a futures order by its client order id";

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IFuturesOrderClientIdRestClient.GetFuturesOrderByClientOrderIdAsync))
        {
        }
    }
}
