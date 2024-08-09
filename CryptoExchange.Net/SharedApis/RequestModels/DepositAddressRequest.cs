using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record DepositAddressRequest
    {
        public string Asset { get; set; }
    }
}
