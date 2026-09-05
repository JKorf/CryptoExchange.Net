using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling all open Futures orders
    /// </summary>
    public class CancelAllFuturesOrdersOptions : CapabilityOptions<CancelAllOrdersRequest, ICancelAllFuturesOrders>
    {
        /// <inheritdoc />
        public override string Description => "Cancel all Futures orders";

        private static readonly RequestParameterDescription[] _defaultParameterRules = [];

        /// <summary>
        /// ctor
        /// </summary>
        public CancelAllFuturesOrdersOptions(string exchange) : base(exchange, true, nameof(ICancelAllFuturesOrders.CancelAllFuturesOrdersAsync), _defaultParameterRules)
        {
        }
    }
}
