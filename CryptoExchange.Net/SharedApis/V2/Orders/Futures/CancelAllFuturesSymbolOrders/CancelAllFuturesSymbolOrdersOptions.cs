using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling all open futures orders for a specific symbol
    /// </summary>
    public class CancelAllFuturesSymbolOrdersOptions : CapabilityOptions<CancelAllSymbolOrdersRequest, ICancelAllFuturesSymbolOrders>
    {
        /// <inheritdoc />
        public override string Description => "Cancel all open futures orders for a specific symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<CancelAllSymbolOrdersRequest>.Required(x => x.Symbol, "The symbol of the orders to cancel", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public CancelAllFuturesSymbolOrdersOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelAllFuturesSymbolOrders.CancelAllFuturesSymbolOrdersAsync), _defaultParameterRules)
        {
        }
    }
}
