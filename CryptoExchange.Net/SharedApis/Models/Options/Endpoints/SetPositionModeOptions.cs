using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.EndpointOptions
{
    public record SetPositionModeOptions : EndpointOptions<SetPositionModeRequest>
    {
        public bool PerSymbol { get; set; }
        public bool SupportsOneway { get; set; }
        public bool SupportsLongShort { get; set; }

        public SetPositionModeOptions(bool supportOneWay, bool supportLongShort, bool perSymbol) : base(true)
        {
            SupportsLongShort = supportLongShort;
            SupportsOneway = supportOneWay;
            PerSymbol = perSymbol;
        }

        public override Error? ValidateRequest(string exchange, SetPositionModeRequest request, TradingMode? apiType, TradingMode[] supportedApiTypes)
        {
            if ((request.Mode == Enums.SharedPositionMode.HedgeMode && !SupportsLongShort)
                || (request.Mode == Enums.SharedPositionMode.OneWay && !SupportsOneway))
                return new ArgumentError($"PositionMode.{request.Mode} is not supported");

            if (request.Symbol == null && PerSymbol)
                return new ArgumentError($"PositionMode is set per symbol, please provide the Symbol parameter");

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
        }

        public string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Position mode per symbol: {PerSymbol}");
            sb.AppendLine($"One way position mode supported: {SupportsOneway}");
            sb.AppendLine($"Hedge position mode supported: {PerSymbol}");
            return sb.ToString();
        }
    }
}
