using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetOpenOrdersRequest
    {
        public ApiType? ApiType { get; set; }
        public SharedSymbol? Symbol { get; set; }

    }
}
