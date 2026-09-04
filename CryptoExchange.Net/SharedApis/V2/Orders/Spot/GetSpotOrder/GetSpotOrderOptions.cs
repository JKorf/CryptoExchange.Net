using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by id endpoint
    /// </summary>
    public class GetSpotOrderOptions : CapabilityOptions<GetOrderRequest, IGetSpotOrderRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a spot order";

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetSpotOrderRest.GetSpotOrderAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetOrderRequest request, IGetSpotOrderRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
