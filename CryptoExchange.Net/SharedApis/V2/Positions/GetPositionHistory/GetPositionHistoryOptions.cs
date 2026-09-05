using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting position history
    /// </summary>
    public class GetPositionHistoryOptions : PaginatedCapabilityOptions<GetPositionHistoryRequest, IGetPositionHistoryRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve historical futures positions";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetPositionHistoryRequest>.Optional(x => x.TradingMode, "Filter the result set by trading mode", TradingMode.PerpetualLinear),
            RequestParameterRule<GetPositionHistoryRequest>.Optional(x => x.Symbol, "Filter the result set by symbol", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<GetPositionHistoryRequest>.Optional(x => x.StartTime, "Filter the result set by start time", DateTime.UtcNow.AddDays(-1)),
            RequestParameterRule<GetPositionHistoryRequest>.Optional(x => x.EndTime, "Filter the result set by end time", DateTime.UtcNow.AddHours(-1)),
            RequestParameterRule<GetPositionHistoryRequest>.Optional(x => x.Limit, "Limit the result set to a maximum number of items", 100),
            RequestParameterRule<GetPositionHistoryRequest>.Optional(x => x.Direction, "The direction in which to retrieve the results", DataDirection.Descending),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionHistoryOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit) 
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, true, nameof(IGetPositionHistoryRest.GetPositionHistoryAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetPositionHistoryRequest request, IGetPositionHistoryRest client)
        {
            if (!SupportsAscending && request.Direction == DataDirection.Ascending)
                return ArgumentError.Invalid(nameof(GetWithdrawalsRequest.Direction), $"Ascending direction is not supported");

            if (!SupportsDescending && request.Direction == DataDirection.Descending)
                return ArgumentError.Invalid(nameof(GetWithdrawalsRequest.Direction), $"Descending direction is not supported");

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return ArgumentError.Invalid(nameof(GetKlinesRequest.StartTime), $"Only the most recent {MaxAge} period data is available");

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
    }
}
