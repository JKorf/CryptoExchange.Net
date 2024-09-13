using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetTradeHistoryOptions : PaginatedEndpointOptions<GetTradeHistoryRequest>
    {
        public TimeSpan? MaxAge { get; set; }

        public GetTradeHistoryOptions(SharedPaginationType paginationType, bool needsAuthentication) : base(paginationType, needsAuthentication)
        {
        }

        public override Error? ValidateRequest(string exchange, GetTradeHistoryRequest request, ApiType? apiType, ApiType[] supportedApiTypes)
        {
            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return new ArgumentError($"Only the most recent {MaxAge} trades are available");

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
        }

        public string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            if (MaxAge != null)
                sb.AppendLine($"Max age of data: {MaxAge}");
            return sb.ToString();
        }
    }
}
