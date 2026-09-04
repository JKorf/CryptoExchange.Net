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


        /// <summary>
        /// ctor
        /// </summary>
        public EditSpotOrderOptions(string exchange) : base(exchange, true, nameof(IEditSpotOrder.EditSpotOrderAsync))
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
