using CryptoExchange.Net.Objects;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting deposits
    /// </summary>
    public class GetDepositsOptions : PaginatedEndpointOptions<GetDepositsRequest>
    {
        /// <summary>
        /// Whether the start/end time filter is supported
        /// </summary>
        public bool TimeFilterSupported { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetDepositsOptions(SharedPaginationSupport paginationType, bool timeFilterSupported) : base(paginationType, true)
        {
            TimeFilterSupported = timeFilterSupported;
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(string exchange, GetDepositsRequest request, TradingMode? tradingMode, TradingMode[] supportedApiTypes)
        {
            if (TimeFilterSupported && request.StartTime != null)
                return new ArgumentError($"Time filter is not supported");

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Time filter supported: {TimeFilterSupported}");
            return sb.ToString();
        }
    }
}
