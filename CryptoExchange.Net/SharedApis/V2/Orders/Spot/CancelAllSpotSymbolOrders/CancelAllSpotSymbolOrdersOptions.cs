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

        /// <summary>
        /// ctor
        /// </summary>
        public CancelAllSpotSymbolOrdersOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelAllSpotSymbolOrders.CancelAllSpotSymbolOrdersAsync))
        {
        }
    }
}
