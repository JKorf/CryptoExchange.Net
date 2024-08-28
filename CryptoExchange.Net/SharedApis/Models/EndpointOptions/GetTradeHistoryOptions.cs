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

        public GetTradeHistoryOptions(bool paginationSupport, bool needsAuthentication) : base(paginationSupport, needsAuthentication)
        {
        }

        public override Error? ValidateRequest(string exchange, GetTradeHistoryRequest request, ExchangeParameters? exchangeParameters)
        {
            if (MaxAge.HasValue && request.StartTime < DateTime.UtcNow.Add(-MaxAge.Value))
                return new ArgumentError($"Only the most recent {MaxAge} trades are available");

            return base.ValidateRequest(exchange, request, exchangeParameters);
        }
    }
}
