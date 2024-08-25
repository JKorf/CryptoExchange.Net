using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetRecentTradesOptions : EndpointOptions<GetRecentTradesRequest>
    {
        public int MaxLimit { get; set; }

        public GetRecentTradesOptions(int limit, bool authenticated) : base(authenticated)
        {
            MaxLimit = limit;
        }

        public Error? Validate(GetRecentTradesRequest request)
        {
            if (request.Limit > MaxLimit)
                return new ArgumentError($"Only the most recent {MaxLimit} trades are available");

            return null;
        }
    }
}
