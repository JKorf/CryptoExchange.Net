using CryptoExchange.Net.Objects;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting recent trades
    /// </summary>
    public class GetRecentTradesOptions : EndpointOptions<GetRecentTradesRequest>
    {
        /// <summary>
        /// The max number of trades that can be requested
        /// </summary>
        public int MaxLimit { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetRecentTradesOptions(int limit, bool authenticated) : base(authenticated)
        {
            MaxLimit = limit;
        }

        /// <inheritdoc />
        public Error? Validate(GetRecentTradesRequest request)
        {
            if (request.Limit > MaxLimit)
                return new ArgumentError($"Only the most recent {MaxLimit} trades are available");

            return null;
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Max data points: {MaxLimit}");
            return sb.ToString();
        }
    }
}
