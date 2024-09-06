using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.EndpointOptions
{
    public record GetPositionModeOptions : EndpointOptions<GetPositionModeRequest>
    {
        public bool PerSymbol { get; set; }

        public GetPositionModeOptions(bool perSymbol) : base(true)
        {
            PerSymbol = perSymbol;
        }

        public override Error? ValidateRequest(string exchange, GetPositionModeRequest request, ExchangeParameters? exchangeParameters, ApiType apiType, ApiType[] supportedApiTypes)
        {
            if (request.Symbol == null && PerSymbol)
                return new ArgumentError($"PositionMode is set per symbol, please provide the Symbol parameter");

            return base.ValidateRequest(exchange, request, exchangeParameters, apiType, supportedApiTypes);
        }
    }
}
