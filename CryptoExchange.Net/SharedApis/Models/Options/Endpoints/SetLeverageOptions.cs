using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models.FilterOptions;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.EndpointOptions
{
    public record SetLeverageOptions : EndpointOptions<SetLeverageRequest>
    {
        public bool LeveragePerSide { get; set; }

        public SetLeverageOptions(bool leveragePerSide) : base(true)
        {
            LeveragePerSide = leveragePerSide;
        }

        public override Error? ValidateRequest(string exchange, SetLeverageRequest request, TradingMode? apiType, TradingMode[] supportedApiTypes)
        {
            if (LeveragePerSide && request.Side == null)
                return new ArgumentError("Leverage needs to be set per side, please provide the PositionSide parameter");

            return base.ValidateRequest(exchange, request, apiType, supportedApiTypes);
        }

        public string ToString(string exchange)
        {
            var sb = new StringBuilder(base.ToString(exchange));
            sb.AppendLine($"Leverage set per side: {LeveragePerSide}");
            return sb.ToString();
        }
    }
}
