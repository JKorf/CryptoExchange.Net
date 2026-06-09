using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot trigger order
    /// </summary>
    public class PlaceSpotTriggerOrderOptions : EndpointOptions<PlaceSpotTriggerOrderRequest, ISpotOrderRestClient>
    {
        /// <summary>
        /// When true the API holds the funds until the order is triggered or canceled. When false the funds will only be required when the order is triggered and will fail if the funds are not available at that time.
        /// </summary>
        public bool HoldsFunds { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceSpotTriggerOrderOptions(string exchange, bool holdsFunds) : base(exchange, true, nameof(ISpotTriggerOrderRestClient.PlaceSpotTriggerOrderAsync))
        {
            HoldsFunds = holdsFunds;
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        public override Error? ValidateRequest(
            PlaceSpotTriggerOrderRequest request,
            ISpotOrderRestClient client)
        {
            var quantityError = client.SpotSupportedOrderQuantity.Validate(request.OrderSide, request.OrderPrice == null ? SharedOrderType.Market : SharedOrderType.Limit, request.Quantity);
            if (quantityError != null)
                return quantityError;

            return base.ValidateRequest(request, client);
        }
    }
}
