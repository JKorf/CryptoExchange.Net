using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting trade history
    /// </summary>
    public class GetTradeHistoryOptions : PaginatedEndpointOptions<GetTradeHistoryRequest, ITradeHistoryRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetTradeHistoryOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit, bool needsAuthentication)
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, needsAuthentication, nameof(ITradeHistoryRestClient.GetTradeHistoryAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetTradeHistoryRequest request, ITradeHistoryRestClient client)
        {
            if (!SupportsAscending && request.Direction == DataDirection.Ascending)
                return ArgumentError.Invalid(nameof(GetTradeHistoryRequest.Direction), $"Ascending direction is not supported");

            if (!SupportsDescending && request.Direction == DataDirection.Descending)
                return ArgumentError.Invalid(nameof(GetTradeHistoryRequest.Direction), $"Descending direction is not supported");

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return ArgumentError.Invalid(nameof(GetTradeHistoryRequest.StartTime), $"Only the most recent {MaxAge} period data is available");

            return base.ValidateRequest(request, client);
        }
    }
}
