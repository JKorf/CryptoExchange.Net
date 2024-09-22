using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Options.Endpoints
{
    /// <summary>
    /// Options for requesting withdrawals
    /// </summary>
    public class GetWithdrawalsOptions : PaginatedEndpointOptions<GetWithdrawalsRequest>
    {
        /// <summary>
        /// Whether the start/end time filter is supported
        /// </summary>
        public bool TimeFilterSupported { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetWithdrawalsOptions(SharedPaginationSupport paginationType, bool timeFilterSupported) : base(paginationType, true)
        {
            TimeFilterSupported = timeFilterSupported;
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(string exchange, GetWithdrawalsRequest request, TradingMode? apiType, TradingMode[] supportedApiTypes)
        {
            if (TimeFilterSupported && request.StartTime != null)
                return new ArgumentError($"Time filter is not supported");

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
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
