using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedOrderId
    {
        public string OrderId { get; set; }

        public SharedOrderId(string orderId)
        {
            OrderId = orderId;
        }
    }
}
