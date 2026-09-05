using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for getting a spot order by client order id endpoint
    /// </summary>
    public class GetSpotOrderByClientOrderIdOptions : CapabilityOptions<GetOrderRequest, IGetSpotOrderByClientOrderIdRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a spot order by its client order id";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOrderRequest>.Required(x => x.Symbol, "The symbol of the order to retrieve", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
            RequestParameterRule<GetOrderRequest>.Required(x => x.OrderId, "The client order id of the order to retrieve", "123")
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetSpotOrderByClientOrderIdOptions(string exchange, bool authenticated)
            : base(exchange, authenticated, nameof(IGetSpotOrderByClientOrderIdRest.GetSpotOrderByClientOrderIdAsync), _defaultParameterRules)
        {
        }

        /// <inheritdoc />
        public override Error? ValidateRequest(GetOrderRequest request, IGetSpotOrderByClientOrderIdRest client)
        {
            if (request.Symbol!.TradingMode != TradingMode.Spot)
                return ArgumentError.Invalid("TradingMode", $"TradingMode.{request.Symbol!.TradingMode} is not supported, should be Spot");

            return base.ValidateRequest(request, client);
        }
    }
}
