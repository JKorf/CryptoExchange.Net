using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedId
    {
        public string Id { get; set; }

        public SharedId(string id)
        {
            Id = id;
        }
    }
}
