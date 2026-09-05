using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting futures trigger order
    /// </summary>
    public class GetFuturesTriggerOrderOptions : CapabilityOptions<GetOrderRequest, IGetFuturesTriggerOrderRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve a futures trigger order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetOrderRequest>.Required(x => x.Symbol, "The symbol of the order to retrieve", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<GetOrderRequest>.Required(x => x.OrderId, "The id of the order to retrieve", "123"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetFuturesTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetFuturesTriggerOrderRest.GetFuturesTriggerOrderAsync), _defaultParameterRules)
        {
        }
    }
}
