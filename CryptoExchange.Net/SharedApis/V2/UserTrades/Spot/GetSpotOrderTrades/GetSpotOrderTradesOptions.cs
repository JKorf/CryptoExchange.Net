using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting trades for a spot order
    /// </summary>
    public class GetSpotOrderTradesOptions : CapabilityOptions<GetOrderTradesRequest, IGetSpotOrderTradesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve trades for a spot order";

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotOrderTradesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetSpotOrderTradesRest.GetSpotOrderTradesAsync))
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetOrderTradesRequest request, IGetSpotOrderTradesRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
