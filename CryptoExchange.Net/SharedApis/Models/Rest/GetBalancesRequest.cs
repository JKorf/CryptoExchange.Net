using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetBalancesRequest
    {
        public ApiType? ApiType { get; set; }

        public GetBalancesRequest(ApiType? apiType = null)
        {
            ApiType = apiType;
        }
    }
}
