using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting spot trigger order
    /// </summary>
    public class CancelSpotTriggerOrderOptions : CapabilityOptions<CancelOrderRequest, ICancelSpotTriggerOrderRest>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a spot trigger order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<CancelOrderRequest>.Required(x => x.Symbol, "The symbol of the order to cancel", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<CancelOrderRequest>.Required(x => x.OrderId, "The id of the order to cancel", "123"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelSpotTriggerOrderRest.CancelSpotTriggerOrderAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(CancelOrderRequest request, ICancelSpotTriggerOrderRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
