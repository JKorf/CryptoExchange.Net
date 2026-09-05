using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot trigger order
    /// </summary>
    public class PlaceFuturesTriggerOrderOptions : CapabilityOptions<PlaceFuturesTriggerOrderRequest, IPlaceFuturesTriggerOrderRest>
    {
        /// <inheritdoc />
        public override string Description => "Place a new futures trigger order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Required(x => x.Symbol, "The symbol to place the trigger order on", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Optional(x => x.ClientOrderId, "The client order id", "123"),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Required(x => x.OrderDirection, "The direction of the order when triggered", SharedTriggerOrderDirection.Enter),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Required(x => x.PriceDirection, "The price direction which activates the order", SharedTriggerPriceDirection.PriceAbove),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Required(x => x.Quantity, "The order quantity", SharedQuantity.Base(0.1m)),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Optional(x => x.OrderPrice, "The limit price of the order", 1m),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Required(x => x.TriggerPrice, "The price at which the order activates", 1m),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Optional(x => x.TimeInForce, "The order time in force", SharedTimeInForce.GoodTillCanceled),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Optional(x => x.PositionMode, "The position mode of the account", SharedPositionMode.OneWay),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Required(x => x.PositionSide, "The position side of the order", SharedPositionSide.Long),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Optional(x => x.MarginMode, "The margin mode of the order", SharedMarginMode.Cross),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Optional(x => x.Leverage, "The leverage for the position", 10m),
            RequestParameterRule<PlaceFuturesTriggerOrderRequest>.Optional(x => x.TriggerPriceType, "The price type used to trigger the order", SharedTriggerPriceType.LastPrice),
        };

        /// <summary>
        /// When true the API holds the funds until the order is triggered or canceled. When false the funds will only be required when the order is triggered and will fail if the funds are not available at that time.
        /// </summary>
        public bool HoldsFunds { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceFuturesTriggerOrderOptions(string exchange, bool holdsFunds) : base(exchange, true, nameof(IPlaceFuturesTriggerOrderRest.PlaceFuturesTriggerOrderAsync), _defaultParameterRules)
        {
            HoldsFunds = holdsFunds;
        }
    }
}
