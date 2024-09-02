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

        public override Error? ValidateRequest(string exchange, SetLeverageRequest request, ExchangeParameters? exchangeParameters)
        {
            if (!LeveragePerSide && request.Side != Enums.SharedPositionSide.Both)
                return new ArgumentError("Leverage per position side not supported");

            return base.ValidateRequest(exchange, request, exchangeParameters);
        }
    }
}
