using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting ticker
    /// </summary>
    public class GetTickerOptions<TClient> : EndpointOptions<GetTickerRequest, TClient>
        where TClient : ISharedClient
    {
        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false)
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

    /// <summary>
    /// Options for requesting ticker
    /// </summary>
    public class GetSpotTickerOptions : GetTickerOptions<ISpotTickerRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, tickerCalcType)
        {
        }
    }

    /// <summary>
    /// Options for requesting ticker
    /// </summary>
    public class GetFuturesTickerOptions : GetTickerOptions<IFuturesTickerRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, tickerCalcType)
        {
        }
    }
}
