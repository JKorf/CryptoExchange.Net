using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting withdrawals
    /// </summary>
    public class GetWithdrawalsOptions : PaginatedEndpointOptions<GetWithdrawalsRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetWithdrawalsOptions(bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit)
            : base(supportsAscending, supportsDescending, timeFilterSupported, maxLimit, true)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(string exchange, GetWithdrawalsRequest request, TradingMode? tradingMode, TradingMode[] supportedApiTypes)
        {
            if (!SupportsAscending && request.Direction == DataDirection.Ascending)
                return ArgumentError.Invalid(nameof(GetWithdrawalsRequest.Direction), $"Ascending direction is not supported");

            if (!SupportsDescending && request.Direction == DataDirection.Descending)
                return ArgumentError.Invalid(nameof(GetWithdrawalsRequest.Direction), $"Descending direction is not supported");

            if (!TimePeriodFilterSupport)
            {
                // When going descending we can still allow startTime filter to limit the results
                var now = DateTime.UtcNow;
                if ((request.Direction != DataDirection.Descending && request.StartTime != null)
                    || (request.EndTime != null && now - request.EndTime > TimeSpan.FromSeconds(5)))
                {
                    return ArgumentError.Invalid(nameof(GetDepositsRequest.StartTime), $"Time filter is not supported");
                }
            }

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
