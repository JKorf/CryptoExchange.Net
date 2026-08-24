using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting spot trigger order
    /// </summary>
    public class CancelSpotTriggerOrderOptions : EndpointOptions<CancelOrderRequest, ICancelSpotTriggerOrderRestClient>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a spot trigger order";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelSpotTriggerOrderRestClient.CancelSpotTriggerOrderAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(CancelOrderRequest request, ICancelSpotTriggerOrderRestClient client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
