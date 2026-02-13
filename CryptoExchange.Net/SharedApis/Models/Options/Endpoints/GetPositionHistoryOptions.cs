using CryptoExchange.Net.Objects;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting position history
    /// </summary>
    public class GetPositionHistoryOptions : PaginatedEndpointOptions<GetPositionHistoryRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionHistoryOptions(bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit) 
            : base(supportsAscending, supportsDescending, timeFilterSupported, maxLimit, true)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(string exchange, GetPositionHistoryRequest request, TradingMode? tradingMode, TradingMode[] supportedApiTypes)
        {
            if (!TimePeriodFilterSupport && request.StartTime != null)
                return ArgumentError.Invalid(nameof(GetDepositsRequest.StartTime), $"Time filter is not supported");

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Time filter supported: {TimePeriodFilterSupport}");
            return sb.ToString();
        }
    }
}
