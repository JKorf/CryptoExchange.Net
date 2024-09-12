
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetTickersRequest
    {
        public ApiType? ApiType { get; set; }

        public GetTickersRequest(ApiType? apiType = null)
        {
            ApiType = apiType;
        }
    }
}
