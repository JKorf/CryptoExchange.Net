using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record EndpointOptions
    {
        public bool PaginationSupport { get; }

        public EndpointOptions(bool paginationSupport)
        {
            PaginationSupport = paginationSupport;
        }
    }
}
