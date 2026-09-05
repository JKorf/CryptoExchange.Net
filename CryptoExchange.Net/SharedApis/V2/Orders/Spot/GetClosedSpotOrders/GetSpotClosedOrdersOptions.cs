using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting user trades
    /// </summary>
    public class GetSpotClosedOrdersOptions : PaginatedCapabilityOptions<GetClosedOrdersRequest, IGetClosedSpotOrdersRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve closed spot orders";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetClosedOrdersRequest>.Required(x => x.Symbol, "The symbol to retrieve closed spot orders for", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<GetClosedOrdersRequest>.Optional(x => x.StartTime, "Filter the result set by start time", DateTime.UtcNow.AddDays(-1)),
            RequestParameterRule<GetClosedOrdersRequest>.Optional(x => x.EndTime, "Filter the result set by end time", DateTime.UtcNow.AddHours(-1)),
            RequestParameterRule<GetClosedOrdersRequest>.Optional(x => x.Limit, "Limit the result set to a maximum number of items", 100),
            RequestParameterRule<GetClosedOrdersRequest>.Optional(x => x.Direction, "The direction in which to retrieve the results", DataDirection.Descending),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotClosedOrdersOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit)
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, true, nameof(IGetClosedSpotOrdersRest.GetClosedSpotOrdersAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetClosedOrdersRequest request, IGetClosedSpotOrdersRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

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
