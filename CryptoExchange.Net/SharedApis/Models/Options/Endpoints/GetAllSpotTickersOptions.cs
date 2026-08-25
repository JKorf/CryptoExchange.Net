using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting tickers
    /// </summary>
    public class GetAllSpotTickersOptions : EndpointOptions<GetTickersRequest, IGetAllSpotTickersEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve price ticker information for all spot symbols";

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetAllSpotTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(IGetAllSpotTickersEndpoint.GetAllSpotTickersAsync))
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
