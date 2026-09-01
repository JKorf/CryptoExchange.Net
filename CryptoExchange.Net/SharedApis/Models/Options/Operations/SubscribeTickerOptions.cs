namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to ticker updates
    /// </summary>
    public class SubscribeTickerOptions : CapabilityOptions<SubscribeTickerRequest, ISubscribeTickerSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to price ticker updates for a symbol";

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(ISubscribeTickerSocket.SubscribeToTickerUpdatesAsync))
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }
    }
}
