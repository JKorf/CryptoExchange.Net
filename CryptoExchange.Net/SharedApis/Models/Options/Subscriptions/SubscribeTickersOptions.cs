namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to ticker updates
    /// </summary>
    public class SubscribeTickersOptions : EndpointOptions<SubscribeAllTickersRequest, ITickersSocketClient>
    {
        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false)
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }
    }
}
