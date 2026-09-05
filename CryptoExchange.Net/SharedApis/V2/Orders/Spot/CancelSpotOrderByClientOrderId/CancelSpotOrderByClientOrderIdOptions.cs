using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a spot order
    /// </summary>
    public class CancelSpotOrderByClientOrderIdOptions : CapabilityOptions<CancelOrderRequest, ICancelSpotOrderByClientOrderIdRest>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a spot order by its client order id";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<CancelOrderRequest>.Required(x => x.Symbol, "The symbol of the order to cancel", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<CancelOrderRequest>.Required(x => x.OrderId, "The client order id of the order to cancel", "123")
        };

        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotOrderByClientOrderIdOptions(string exchange, bool authenticated)
            : base(exchange, authenticated, nameof(ICancelSpotOrderByClientOrderIdRest.CancelSpotOrderByClientOrderIdAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(CancelOrderRequest request, ICancelSpotOrderByClientOrderIdRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
