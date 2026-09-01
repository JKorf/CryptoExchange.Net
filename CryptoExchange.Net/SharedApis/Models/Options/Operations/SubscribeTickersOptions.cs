namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to ticker updates
    /// </summary>
    public class SubscribeTickersOptions : CapabilityOptions<SubscribeAllTickersRequest, ISubscribeAllTickersSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to price ticker updates for all symbols";

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(ISubscribeAllTickersSocket.SubscribeToAllTickersUpdatesAsync))
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }
    }
}
