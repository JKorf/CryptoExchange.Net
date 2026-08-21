using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting spot trigger order
    /// </summary>
    public class GetSpotTriggerOrderOptions : EndpointOptions<GetOrderRequest, ISpotTriggerOrderRestClient>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a spot trigger order";

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISpotTriggerOrderRestClient.GetSpotTriggerOrderAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetOrderRequest request, ISpotTriggerOrderRestClient client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
