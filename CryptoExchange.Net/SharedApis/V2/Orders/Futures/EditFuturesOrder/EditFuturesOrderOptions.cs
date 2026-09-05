using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for editing an open Futures order
    /// </summary>
    public class EditFuturesOrderOptions : CapabilityOptions<EditFuturesOrderRequest, IEditFuturesOrder>
    {
        /// <inheritdoc />
        public override string Description => "Edit an existing Futures order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<EditFuturesOrderRequest>.Required(x => x.Symbol, "The symbol of the order to edit", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<EditFuturesOrderRequest>.Required(x => x.OrderId, "The id of the order to edit", "123"),
            RequestParameterRule<EditFuturesOrderRequest>.Optional(x => x.Quantity, "The new order quantity", SharedQuantity.Base(1)),
            RequestParameterRule<EditFuturesOrderRequest>.Optional(x => x.Price, "The new order price", 0.1m),
        };


        /// <summary>
        /// ctor
        /// </summary>
        public EditFuturesOrderOptions(string exchange) : base(exchange, true, nameof(IEditFuturesOrder.EditFuturesOrderAsync), _defaultParameterRules)
        {
        }
    }
}
