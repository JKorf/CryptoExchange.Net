using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting user trades
    /// </summary>
    public class GetUserTradesOptions<TClient> : PaginatedEndpointOptions<GetUserTradesRequest, TClient>
        where TClient : ISharedClient
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetUserTradesOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit) 
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, true)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetUserTradesRequest request, TClient client)
        {
            if (!SupportsAscending && request.Direction == DataDirection.Ascending)
                return ArgumentError.Invalid(nameof(GetUserTradesRequest.Direction), $"Ascending direction is not supported");

            if (!SupportsDescending && request.Direction == DataDirection.Descending)
                return ArgumentError.Invalid(nameof(GetUserTradesRequest.Direction), $"Descending direction is not supported");

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return ArgumentError.Invalid(nameof(GetUserTradesRequest.StartTime), $"Only the most recent {MaxAge} period data is available");

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

            return base.ValidateRequest(request, client);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.AppendLine($"Time filter supported: {TimePeriodFilterSupport}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Options for requesting user trades
    /// </summary>
    public class GetSpotUserTradesOptions : GetUserTradesOptions<ISpotOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotUserTradesOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit)
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit)
        {
        }
    }

    /// <summary>
    /// Options for requesting user trades
    /// </summary>
    public class GetFuturesUserTradesOptions : GetUserTradesOptions<IFuturesOrderRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesUserTradesOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit)
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit)
        {
        }
    }
}
