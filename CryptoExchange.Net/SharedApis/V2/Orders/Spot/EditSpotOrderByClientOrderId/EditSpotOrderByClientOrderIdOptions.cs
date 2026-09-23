namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for editing a spot order by client order id
    /// </summary>
    public class EditSpotOrderByClientOrderIdOptions : CapabilityOptions<EditOrderRequest, IEditSpotOrderByClientOrderId>
    {
        /// <inheritdoc />
        public override string Description => "Edit a spot order by its client order id";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<EditOrderRequest>.Required(x => x.Symbol, "The symbol of the order to edit", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<EditOrderRequest>.Required(x => x.OrderId, "The client order id of the order to edit", "123"),
            RequestParameterRule<EditOrderRequest>.Optional(x => x.Quantity, "The new order quantity", SharedQuantity.Base(1)),
            RequestParameterRule<EditOrderRequest>.Optional(x => x.Price, "The new order price", 0.1m),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public EditSpotOrderByClientOrderIdOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IEditSpotOrderByClientOrderId.EditSpotOrderByClientOrderIdAsync), _defaultParameterRules, SharedTradingModeSets.Spot)
        {
        }
    }
}
