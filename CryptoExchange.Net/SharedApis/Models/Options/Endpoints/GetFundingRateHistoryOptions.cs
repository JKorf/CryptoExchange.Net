using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.EndpointOptions
{
    public record GetFundingRateHistoryOptions : EndpointOptions<GetFundingRateHistoryRequest>
    {
        public GetFundingRateHistoryOptions(bool needsAuthentication) : base(needsAuthentication)
        {
        }
    }
}
