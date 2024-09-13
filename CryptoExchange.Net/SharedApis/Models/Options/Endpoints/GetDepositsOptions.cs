using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record GetDepositsOptions : PaginatedEndpointOptions<GetDepositsRequest>
    {
        public bool TimeFilterSupported { get; set; }

        public GetDepositsOptions(SharedPaginationType paginationType, bool timeFilterSupported) : base(paginationType, true)
        {
            TimeFilterSupported = timeFilterSupported;
        }

        public override Error? ValidateRequest(string exchange, GetDepositsRequest request, ApiType? apiType, ApiType[] supportedApiTypes)
        {
            if (TimeFilterSupported && request.StartTime != null)
                return new ArgumentError($"Time filter is not supported");

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
        }

        public string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Time filter supported: {TimeFilterSupported}");
            return sb.ToString();
        }
    }
}
