using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting ticker
    /// </summary>
    public class GetSpotTickerOptions : EndpointOptions<GetTickerRequest, ISpotTickerRestClient>
    {
        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(ISpotTickerRestClient.GetSpotTickerAsync))
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
