using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling all open spot orders
    /// </summary>
    public class CancelAllSpotOrdersOptions : CapabilityOptions<CancelAllOrdersRequest, ICancelAllSpotOrders>
    {
        /// <inheritdoc />
        public override string Description => "Cancel all spot orders";

        private static readonly RequestParameterDescription[] _defaultParameterRules = [];

        /// <summary>
        /// ctor
        /// </summary>
        public CancelAllSpotOrdersOptions(string exchange) : base(exchange, true, nameof(ICancelAllSpotOrders.CancelAllSpotOrdersAsync), _defaultParameterRules)
        {
        }
    }
}
