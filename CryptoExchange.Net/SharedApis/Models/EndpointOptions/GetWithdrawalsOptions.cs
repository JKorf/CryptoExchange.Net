using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetWithdrawalsOptions : PaginatedEndpointOptions<GetWithdrawalsRequest>
    {
        public bool TimeFilterSupported { get; set; }

        public GetWithdrawalsOptions(bool paginationSupport, bool timeFilterSupported) : base(paginationSupport, true)
        {
            TimeFilterSupported = timeFilterSupported;
        }

        public override Error? ValidateRequest(string exchange, GetWithdrawalsRequest request, ExchangeParameters? exchangeParameters)
        {
            if (TimeFilterSupported && request.Filter?.StartTime != null)
                return new ArgumentError($"Time filter is not supported");

            return base.ValidateRequest(exchange, request, exchangeParameters);
        }
    }
}
