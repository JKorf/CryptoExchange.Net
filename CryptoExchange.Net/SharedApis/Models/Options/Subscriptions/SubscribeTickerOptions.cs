namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to ticker updates
    /// </summary>
    public class SubscribeTickerOptions : EndpointOptions<SubscribeTickerRequest, ITickerSocketClient>
    {
        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(ITickerSocketClient.SubscribeToTickerUpdatesAsync))
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }
    }
}
