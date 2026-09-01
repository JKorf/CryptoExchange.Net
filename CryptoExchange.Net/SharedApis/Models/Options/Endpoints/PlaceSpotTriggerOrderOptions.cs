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

        /// <summary>
        /// When true the API holds the funds until the order is triggered or canceled. When false the funds will only be required when the order is triggered and will fail if the funds are not available at that time.
        /// </summary>
        public bool HoldsFunds { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceSpotTriggerOrderOptions(string exchange, bool holdsFunds) : base(exchange, true, nameof(IPlaceSpotTriggerOrderRest.PlaceSpotTriggerOrderAsync))
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
