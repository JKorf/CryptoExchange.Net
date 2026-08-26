using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot trigger order
    /// </summary>
    public class PlaceFuturesTriggerOrderOptions : CapabilityOptions<PlaceFuturesTriggerOrderRequest, IPlaceFuturesTriggerOrderEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Place a new futures trigger order";

        /// <summary>
        /// When true the API holds the funds until the order is triggered or canceled. When false the funds will only be required when the order is triggered and will fail if the funds are not available at that time.
        /// </summary>
        public bool HoldsFunds { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceFuturesTriggerOrderOptions(string exchange, bool holdsFunds) : base(exchange, true, nameof(IPlaceFuturesTriggerOrderEndpoint.PlaceFuturesTriggerOrderAsync))
        {
            HoldsFunds = holdsFunds;
        }
    }
}
