using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.EndpointOptions
{
    public record GetFundingRateHistoryOptions : EndpointOptions<GetFundingRateHistoryRequest>
    {
        public bool TimeFilterSupported { get; set; }

        public GetFundingRateHistoryOptions(bool timeFilterSupported, bool needsAuthentication) : base(needsAuthentication)
        {
            TimeFilterSupported = timeFilterSupported;
        }
    }
}
