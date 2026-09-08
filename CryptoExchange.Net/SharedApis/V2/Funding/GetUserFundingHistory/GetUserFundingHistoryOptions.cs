using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting funding rate history
    /// </summary>
    public class GetUserFundingHistoryOptions : PaginatedCapabilityOptions<GetUserFundingHistoryRequest, IGetUserFundingHistoryRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve funding fee payments for the user";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetUserFundingHistoryRequest>.Optional(x => x.Symbol, "Filter by symbol", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<GetUserFundingHistoryRequest>.Optional(x => x.StartTime, "Filter the result set by start time", DateTime.UtcNow.AddDays(-1)),
            RequestParameterRule<GetUserFundingHistoryRequest>.Optional(x => x.EndTime, "Filter the result set by end time", DateTime.UtcNow.AddHours(-1)),
            RequestParameterRule<GetUserFundingHistoryRequest>.Optional(x => x.Limit, "Limit the result set to a maximum number of items", 100),
            RequestParameterRule<GetUserFundingHistoryRequest>.Optional(x => x.Direction, "The direction in which to retrieve the results", DataDirection.Descending),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetUserFundingHistoryOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit, bool needsAuthentication) 
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, needsAuthentication, nameof(IGetUserFundingHistoryRest.GetUserFundingHistoryAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetUserFundingHistoryRequest request, IGetUserFundingHistoryRest client)
        {
            var error = base.ValidateRequest(request, client);
            if (error != null)
                return error;

            if (!SupportsAscending && request.Direction == DataDirection.Ascending)
                return ArgumentError.Invalid(nameof(GetUserFundingHistoryRequest.Direction), $"Ascending direction is not supported");

            if (!SupportsDescending && request.Direction == DataDirection.Descending)
                return ArgumentError.Invalid(nameof(GetUserFundingHistoryRequest.Direction), $"Descending direction is not supported");

            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return ArgumentError.Invalid(nameof(GetUserFundingHistoryRequest.StartTime), $"Only the most recent {MaxAge} period data is available");

            if (!TimePeriodFilterSupport)
            {
                // When going descending we can still allow startTime filter to limit the results
                var now = DateTime.UtcNow;
                if ((request.Direction != DataDirection.Descending && request.StartTime != null)
                    || (request.EndTime != null && now - request.EndTime > TimeSpan.FromSeconds(5)))
                {
                    return ArgumentError.Invalid(nameof(GetUserFundingHistoryRequest.StartTime), $"Time filter is not supported");
                }
            }

            return null;
        }
    }
}
