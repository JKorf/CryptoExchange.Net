using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by client order id endpoint
    /// </summary>
    public class GetSpotOrderByClientOrderIdOptions : EndpointOptions<GetOrderRequest, ISpotOrderClientIdRestClient>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a spot order by its client order id";

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotOrderClientIdRestClient.GetSpotOrderByClientOrderIdAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetOrderRequest request, ISpotOrderClientIdRestClient client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
