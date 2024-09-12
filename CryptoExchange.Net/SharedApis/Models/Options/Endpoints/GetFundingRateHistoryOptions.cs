using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.EndpointOptions
{
    public record GetFundingRateHistoryOptions : PaginatedEndpointOptions<GetFundingRateHistoryRequest>
    {
        public GetFundingRateHistoryOptions(SharedPaginationType paginationType, bool needsAuthentication) : base(paginationType, needsAuthentication)
        {
        }
    }
}
