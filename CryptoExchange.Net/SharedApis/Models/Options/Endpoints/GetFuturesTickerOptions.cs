using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting ticker
    /// </summary>
    public class GetFuturesTickerOptions : CapabilityOptions<GetTickerRequest, IGetFuturesTickerEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve price ticker information for a futures symbol";

        /// <summary>
        /// Type of ticker calculation
        /// </summary>
        public SharedTickerType TickerType { get; set; } = SharedTickerType.Day24H;

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesTickerOptions(string exchange, SharedTickerType? tickerCalcType = null) : base(exchange, false, nameof(IGetFuturesTickerEndpoint.GetFuturesTickerAsync))
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
