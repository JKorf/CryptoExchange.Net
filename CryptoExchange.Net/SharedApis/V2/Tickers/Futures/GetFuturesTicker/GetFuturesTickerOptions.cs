using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting ticker
    /// </summary>
    public class GetFuturesTickerOptions : CapabilityOptions<GetTickerRequest, IGetFuturesTickerRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve price ticker information for a futures symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetTickerRequest>.Required(x => x.Symbol, "The symbol to retrieve ticker information for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(IGetFuturesTickerRest.GetFuturesTickerAsync), _defaultParameterRules)
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine($"  Ticker data calculation type:   {TickerType}");
            return sb.ToString();
        }
    }
}
