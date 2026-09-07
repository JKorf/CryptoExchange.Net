using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting ticker
    /// </summary>
    public class GetTickerOptions : CapabilityOptions<GetTickerRequest, IGetTickerRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve price ticker information for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetTickerRequest>.Required(x => x.Symbol, "The symbol to retrieve ticker information for", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
        };

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(IGetTickerRest.GetTickerAsync), _defaultParameterRules)
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
