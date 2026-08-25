using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a spot order
    /// </summary>
    public class CancelSpotOrderByClientOrderIdOptions : EndpointOptions<CancelOrderRequest, ICancelSpotOrderByClientOrderIdEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a spot order by its client order id";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelSpotOrderByClientOrderIdEndpoint.CancelSpotOrderByClientOrderIdAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(CancelOrderRequest request, ICancelSpotOrderByClientOrderIdEndpoint client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
