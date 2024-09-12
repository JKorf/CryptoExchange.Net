using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record StopListenKeyRequest
    {
        public string ListenKey { get; set; }
        public ApiType? ApiType { get; set; }

        public StopListenKeyRequest(string listenKey, ApiType? apiType = null)
        {
            ListenKey = listenKey;
            ApiType = apiType;
        }
    }
}
