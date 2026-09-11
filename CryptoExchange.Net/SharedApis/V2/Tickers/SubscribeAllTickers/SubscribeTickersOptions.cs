namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to ticker updates
    /// </summary>
    public class SubscribeTickersOptions : CapabilityOptions<SubscribeAllTickersRequest, ISubscribeAllTickersSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to price ticker updates for all symbols";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeAllTickersRequest>.Optional(x => x.TradingMode, "Filter ticker updates by trading mode", TradingMode.Spot),
        };

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(ISubscribeAllTickersSocket.SubscribeToAllTickersUpdatesAsync), _defaultParameterRules)
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }
    }
}
