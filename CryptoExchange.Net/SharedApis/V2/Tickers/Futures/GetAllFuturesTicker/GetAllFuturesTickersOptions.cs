using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting tickers
    /// </summary>
    public class GetAllFuturesTickersOptions : CapabilityOptions<GetTickersRequest, IGetAllFuturesTickersRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve price ticker information for all futures symbols";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetTickersRequest>.Optional(x => x.TradingMode, "Filter the tickers by trading mode", TradingMode.PerpetualLinear),
        };

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetAllFuturesTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(IGetAllFuturesTickersRest.GetAllFuturesTickersAsync), _defaultParameterRules)
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
