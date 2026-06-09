using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting tickers
    /// </summary>
    public class GetSpotTickersOptions : EndpointOptions<GetTickersRequest, ISpotTickerRestClient>
    {
        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(ISpotTickerRestClient.GetSpotTickersAsync))
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine($"Ticker data calculation type: {TickerType}");
            return sb.ToString();
        }
    }
}
