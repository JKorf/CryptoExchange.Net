using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for editing an open spot order
    /// </summary>
    public class EditSpotOrderOptions : CapabilityOptions<EditSpotOrderRequest, IEditSpotOrder>
    {
        /// <inheritdoc />
        public override string Description => "Edit an existing spot order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<EditSpotOrderRequest>.Required(x => x.Symbol, "The symbol of the order to edit", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<EditSpotOrderRequest>.Required(x => x.OrderId, "The order id of the order to edit", "123"),
            RequestParameterRule<EditSpotOrderRequest>.Optional(x => x.Quantity, "The new order quantity", SharedQuantity.Base(1)),
            RequestParameterRule<EditSpotOrderRequest>.Optional(x => x.Price, "The new order price", 0.1m),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public EditSpotOrderOptions(string exchange)
            : base(exchange, true, nameof(IEditSpotOrder.EditSpotOrderAsync), _defaultParameterRules)
        {
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            EditSpotOrderRequest request,
            IEditSpotOrder client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
