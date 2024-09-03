using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.EndpointOptions
{
    public record GetClosedOrdersOptions : PaginatedEndpointOptions<GetClosedOrdersRequest>
    {
        public bool TimeFilterSupported { get; set; }

        public GetClosedOrdersOptions(bool paginationSupport, bool timeFilterSupported) : base(paginationSupport, true)
        {
            TimeFilterSupported = timeFilterSupported;
        }

        public override Error? ValidateRequest(string exchange, GetClosedOrdersRequest request, ExchangeParameters? exchangeParameters)
        {
            if (TimeFilterSupported && request.Filter?.StartTime != null)
                return new ArgumentError($"Time filter is not supported");

            return base.ValidateRequest(exchange, request, exchangeParameters);
        }
    }
}
