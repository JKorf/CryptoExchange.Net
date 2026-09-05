using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling all open Spot orders for a specific symbol
    /// </summary>
    public class CancelAllSpotSymbolOrdersOptions : CapabilityOptions<CancelAllSymbolOrdersRequest, ICancelAllSpotSymbolOrders>
    {
        /// <inheritdoc />
        public override string Description => "Cancel all open Spot orders for a specific symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<CancelAllSymbolOrdersRequest>.Required(x => x.Symbol, "The symbol of the orders to cancel", new SharedSymbol(TradingMode.Spot, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public CancelAllSpotSymbolOrdersOptions(string exchange, bool authenticated)
            : base(exchange, authenticated, nameof(ICancelAllSpotSymbolOrders.CancelAllSpotSymbolOrdersAsync), _defaultParameterRules)
        {
        }
    }
}
