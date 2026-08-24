using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a spot order
    /// </summary>
    public class CancelSpotOrderOptions : EndpointOptions<CancelOrderRequest, ICancelSpotOrderRestClient>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a spot order";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelSpotOrderRestClient.CancelSpotOrderAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(CancelOrderRequest request, ICancelSpotOrderRestClient client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
