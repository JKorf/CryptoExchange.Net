using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record StartListenKeyRequest
    {
        public ApiType? ApiType { get; set; }

        public StartListenKeyRequest(ApiType? apiType = null)
        {
            ApiType = apiType;
        }
    }
}
