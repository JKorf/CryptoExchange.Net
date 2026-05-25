using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting tickers
    /// </summary>
    public class GetTickersOptions<TClient> : EndpointOptions<GetTickersRequest, TClient>
        where TClient : ISharedClient
    {
        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false)
        {
            TickerType = tickerCalcType ?? SharedTickerType.Day24H;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine($"Ticker time calc type: {TickerType}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Options for requesting tickers
    /// </summary>
    public class GetSpotTickersOptions : GetTickersOptions<ISpotTickerRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, tickerCalcType)
        {
        }
    }

    /// <summary>
    /// Options for requesting tickers
    /// </summary>
    public class GetFuturesTickersOptions : GetTickersOptions<IFuturesTickerRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesTickersOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, tickerCalcType)
        {
        }
    }
}
