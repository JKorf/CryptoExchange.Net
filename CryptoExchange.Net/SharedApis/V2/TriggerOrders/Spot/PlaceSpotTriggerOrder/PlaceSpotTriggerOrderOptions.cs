using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot trigger order
    /// </summary>
    public class PlaceSpotTriggerOrderOptions : CapabilityOptions<PlaceSpotTriggerOrderRequest, IPlaceSpotTriggerOrderRest>
    {
        /// <inheritdoc />
        public override string Description => "Place a new spot trigger order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<PlaceSpotTriggerOrderRequest>.Required(x => x.Symbol, "The symbol to place the trigger order on", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<PlaceSpotTriggerOrderRequest>.Optional(x => x.ClientOrderId, "The client order id", "123"),
            RequestParameterRule<PlaceSpotTriggerOrderRequest>.Required(x => x.OrderSide, "The order side", SharedOrderSide.Buy),
            RequestParameterRule<PlaceSpotTriggerOrderRequest>.Required(x => x.PriceDirection, "The price direction which activates the order", SharedTriggerPriceDirection.PriceAbove),
            RequestParameterRule<PlaceSpotTriggerOrderRequest>.Optional(x => x.TimeInForce, "The order time in force", SharedTimeInForce.GoodTillCanceled),
            RequestParameterRule<PlaceSpotTriggerOrderRequest>.Required(x => x.Quantity, "The order quantity", SharedQuantity.Base(0.1m)),
            RequestParameterRule<PlaceSpotTriggerOrderRequest>.Optional(x => x.OrderPrice, "The limit price of the order", 1m),
            RequestParameterRule<PlaceSpotTriggerOrderRequest>.Required(x => x.TriggerPrice, "The price at which the order activates", 1m),
        };

        /// <summary>
        /// When true the API holds the funds until the order is triggered or canceled. When false the funds will only be required when the order is triggered and will fail if the funds are not available at that time.
        /// </summary>
        public bool HoldsFunds { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceSpotTriggerOrderOptions(string exchange, bool holdsFunds) : base(exchange, true, nameof(IPlaceSpotTriggerOrderRest.PlaceSpotTriggerOrderAsync), _defaultParameterRules)
        {
            HoldsFunds = holdsFunds;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceSpotTriggerOrderRequest request,
            IPlaceSpotTriggerOrderRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
