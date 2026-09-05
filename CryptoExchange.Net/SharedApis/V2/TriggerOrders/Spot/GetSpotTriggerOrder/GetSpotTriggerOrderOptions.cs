using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting spot trigger order
    /// </summary>
    public class GetSpotTriggerOrderOptions : CapabilityOptions<GetOrderRequest, IGetSpotTriggerOrderRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a spot trigger order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOrderRequest>.Required(x => x.Symbol, "The symbol of the order to retrieve", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<GetOrderRequest>.Required(x => x.OrderId, "The id of the order to retrieve", "123"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetSpotTriggerOrderRest.GetSpotTriggerOrderAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetOrderRequest request, IGetSpotTriggerOrderRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
