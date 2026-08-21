using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a spot order
    /// </summary>
    public class CancelSpotOrderSocketOptions : EndpointOptions<CancelOrderRequest, ISpotOrderManagementSocketClient>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a spot order over a socket connection";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotOrderSocketOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotOrderManagementSocketClient.CancelSpotOrderAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(CancelOrderRequest request, ISpotOrderManagementSocketClient client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
