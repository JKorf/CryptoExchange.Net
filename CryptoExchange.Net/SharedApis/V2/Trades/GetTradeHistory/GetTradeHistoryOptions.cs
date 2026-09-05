using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting trade history
    /// </summary>
    public class GetTradeHistoryOptions : PaginatedCapabilityOptions<GetTradeHistoryRequest, IGetTradeHistoryRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve public trade history for a symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetTradeHistoryRequest>.Required(x => x.Symbol, "The symbol to retrieve trade history for", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<GetTradeHistoryRequest>.Required(x => x.StartTime, "Filter the result set by start time", DateTime.UtcNow.AddDays(-1)),
            RequestParameterRule<GetTradeHistoryRequest>.Optional(x => x.EndTime, "Filter the result set by end time", DateTime.UtcNow.AddHours(-1)),
            RequestParameterRule<GetTradeHistoryRequest>.Optional(x => x.Limit, "Limit the result set to a maximum number of items", 100),
            RequestParameterRule<GetTradeHistoryRequest>.Optional(x => x.Direction, "The direction in which to retrieve the results", DataDirection.Descending),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetTradeHistoryOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit, bool needsAuthentication)
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, needsAuthentication, nameof(IGetTradeHistoryRest.GetTradeHistoryAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetTradeHistoryRequest request, IGetTradeHistoryRest client)
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
