namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for subscribing to ticker updates
    /// </summary>
    public class SubscribeTickerOptions : CapabilityOptions<SubscribeTickerRequest, ISubscribeTickerSocket>
    {
        /// <inheritdoc />
        public override string Description => "Subscribe to price ticker updates for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SubscribeTickerRequest>.Optional(x => x.Symbol, "The symbol to subscribe to", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<SubscribeTickerRequest>.Optional(x => x.Symbols, "The symbols to subscribe to", new[] { new SharedSymbol(TradingMode.Spot, "ETH", "USDT") }),
        };

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SubscribeTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(ISubscribeTickerSocket.SubscribeToTickerUpdatesAsync), _defaultParameterRules)
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }
    }
}
