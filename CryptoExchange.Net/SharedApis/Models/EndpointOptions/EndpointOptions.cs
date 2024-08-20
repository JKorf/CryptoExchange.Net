using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record EndpointOptions
    {
        public bool PaginationSupport { get; }

        // parameters which are optional in the request, but required for this exchange
        public Dictionary<string, string> RequiredOptionalParameters { get; } = new Dictionary<string, string>();
        // parameters which aren't defined in the request, but required for this exchange
        public Dictionary<string, string> RequiredExchangeParameters { get; } = new Dictionary<string, string>();
        // Exchange specific request info
        public string RequestInfo { get; set; }

#warning apply to all endpoings. Pagination probably doesn't fit here, should be a sub class

        public EndpointOptions(bool paginationSupport)
        {
            PaginationSupport = paginationSupport;
        }
    }
}
