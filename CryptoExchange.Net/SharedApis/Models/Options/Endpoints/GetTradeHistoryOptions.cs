using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting trade history
    /// </summary>
    public class GetTradeHistoryOptions : PaginatedEndpointOptions<GetTradeHistoryRequest>
    {
        /// <summary>
        /// The max age of data that can be requested
        /// </summary>
        public TimeSpan? MaxAge { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public GetTradeHistoryOptions(bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit, bool needsAuthentication)
            : base(supportsAscending, supportsDescending, timeFilterSupported, maxLimit, needsAuthentication)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(string exchange, GetTradeHistoryRequest request, TradingMode? tradingMode, TradingMode[] supportedApiTypes)
        {
            if (!SupportsAscending && request.Direction == DataDirection.Ascending)
                return ArgumentError.Invalid(nameof(GetWithdrawalsRequest.Direction), $"Ascending direction is not supported");

            if (!SupportsDescending && request.Direction == DataDirection.Descending)
                return ArgumentError.Invalid(nameof(GetWithdrawalsRequest.Direction), $"Descending direction is not supported");

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return ArgumentError.Invalid(nameof(GetTradeHistoryRequest.StartTime), $"Only the most recent {MaxAge} trades are available");

            return base.ValidateRequest(exchange, request, tradingMode, supportedApiTypes);
        }

        /// <inheritdoc />
        public override string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            if (MaxAge != null)
                sb.AppendLine($"Max age of data: {MaxAge}");
            return sb.ToString();
        }
    }
}
