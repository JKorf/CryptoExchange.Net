using CryptoExchange.Net.Objects;
using System;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting user trades
    /// </summary>
    public class GetSpotUserTradeHistoryOptions : PaginatedCapabilityOptions<GetUserTradesRequest, IGetSpotUserTradeHistoryRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve spot user trade history";

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotUserTradeHistoryOptions(string exchange, bool supportsAscending, bool supportsDescending, bool timeFilterSupported, int maxLimit) 
            : base(exchange, supportsAscending, supportsDescending, timeFilterSupported, maxLimit, true, nameof(IGetSpotUserTradeHistoryRest.GetSpotUserTradeHistoryAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetUserTradesRequest request, IGetSpotUserTradeHistoryRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

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
    }
}
