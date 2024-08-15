using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record EndpointOptions
    {
        public bool TimeFilterSupport { get; }
        public bool PaginationSupport { get; }

        public EndpointOptions(bool timeFilterSupport, bool paginationSupport)
        {
            TimeFilterSupport = timeFilterSupport;
            PaginationSupport = paginationSupport;
        }
    }
}
