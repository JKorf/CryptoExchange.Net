using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.EndpointOptions
{
    public record GetPositionHistoryOptions : PaginatedEndpointOptions<GetPositionHistoryRequest>
    {
        public bool PerSymbol { get; set; }

        public GetPositionHistoryOptions(bool perSymbol, SharedPaginationType paginationType) : base(paginationType, true)
        {
            PerSymbol = perSymbol;
        }

        public override Error? ValidateRequest(string exchange, GetPositionHistoryRequest request, TradingMode? apiType, TradingMode[] supportedApiTypes)
        {
            if (request.Symbol == null && PerSymbol)
                return new ArgumentError($"Position history can only be retrieved per symbol, please provide the Symbol parameter");

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
        }

        public string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Position history per symbol: {PerSymbol}");
            return sb.ToString();
        }
    }
}
