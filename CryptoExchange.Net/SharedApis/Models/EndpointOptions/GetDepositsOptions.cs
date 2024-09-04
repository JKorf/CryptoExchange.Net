using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetDepositsOptions : PaginatedEndpointOptions<GetDepositsRequest>
    {
        public bool TimeFilterSupported { get; set; }

        public GetDepositsOptions(bool paginationSupport, bool timeFilterSupported) : base(paginationSupport, true)
        {
            TimeFilterSupported = timeFilterSupported;
        }

        public override Error? ValidateRequest(string exchange, GetDepositsRequest request, ExchangeParameters? exchangeParameters, ApiType apiType, ApiType[] supportedApiTypes)
        {
            if (TimeFilterSupported && request.Filter?.StartTime != null)
                return new ArgumentError($"Time filter is not supported");

            return base.ValidateRequest(exchange, request, exchangeParameters, apiType, supportedApiTypes);
        }
    }
}
