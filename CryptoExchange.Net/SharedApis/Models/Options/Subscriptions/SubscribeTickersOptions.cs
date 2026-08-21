namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to ticker updates
    /// </summary>
    public class SubscribeTickersOptions : EndpointOptions<SubscribeAllTickersRequest, ITickersSocketClient>
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
        public SubscribeTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(ITickersSocketClient.SubscribeToAllTickersUpdatesAsync))
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }
    }
}
