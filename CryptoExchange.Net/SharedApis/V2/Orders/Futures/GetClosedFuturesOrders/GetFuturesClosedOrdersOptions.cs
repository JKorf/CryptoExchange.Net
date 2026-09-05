using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting closed orders
    /// </summary>
    public class GetFuturesClosedOrdersOptions : PaginatedCapabilityOptions<GetClosedOrdersRequest, IGetClosedFuturesOrdersRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve closed futures orders";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetClosedOrdersRequest>.Required(x => x.Symbol, "The symbol to retrieve closed orders for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<GetClosedOrdersRequest>.Optional(x => x.StartTime, "Filter the result set by start time", DateTime.UtcNow.AddDays(-1)),
            RequestParameterRule<GetClosedOrdersRequest>.Optional(x => x.EndTime, "Filter the result set by end time", DateTime.UtcNow.AddHours(-1)),
            RequestParameterRule<GetClosedOrdersRequest>.Optional(x => x.Limit, "Limit the result set to a maximum number of items", 100),
            RequestParameterRule<GetClosedOrdersRequest>.Optional(x => x.Direction, "The direction in which to retrieve the results", DataDirection.Descending),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesClosedOrdersOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit)
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, true, nameof(IGetClosedFuturesOrdersRest.GetClosedFuturesOrdersAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetClosedOrdersRequest request, IGetClosedFuturesOrdersRest client)
        {
            if (!SupportsAscending && request.Direction == DataDirection.Ascending)
                return ArgumentError.Invalid(nameof(GetClosedOrdersRequest.Direction), $"Ascending direction is not supported");

            if (!SupportsDescending && request.Direction == DataDirection.Descending)
                return ArgumentError.Invalid(nameof(GetClosedOrdersRequest.Direction), $"Descending direction is not supported");

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return ArgumentError.Invalid(nameof(GetClosedOrdersRequest.StartTime), $"Only the most recent {MaxAge} period data is available");

            if (!TimePeriodFilterSupport)
            {
                // When going descending we can still allow startTime filter to limit the results
                var now = DateTime.UtcNow;
                if ((request.Direction != DataDirection.Descending && request.StartTime != null)
                    || (request.EndTime != null && now - request.EndTime > TimeSpan.FromSeconds(5)))
                {
                    return ArgumentError.Invalid(nameof(GetClosedOrdersRequest.StartTime), $"Time filter is not supported");
                }
            }

            return base.ValidateRequest(request, client);
        }
    }

}
