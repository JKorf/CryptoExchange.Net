using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a futures order
    /// </summary>
    public class CancelFuturesOrderOptions : CapabilityOptions<CancelOrderRequest, ICancelFuturesOrder>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a futures order";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<CancelOrderRequest>.Required(x => x.Symbol, "The symbol of the order to cancel", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<CancelOrderRequest>.Required(x => x.OrderId, "The id of the order to cancel", "123"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelFuturesOrder.CancelFuturesOrderAsync), _defaultParameterRules)
        {
        }
    }
}
