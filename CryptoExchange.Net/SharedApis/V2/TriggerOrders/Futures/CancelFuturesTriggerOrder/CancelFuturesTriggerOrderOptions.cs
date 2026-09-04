using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling spot trigger order
    /// </summary>
    public class CancelFuturesTriggerOrderOptions : CapabilityOptions<CancelOrderRequest, ICancelFuturesTriggerOrderRest>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a futures trigger order";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesTriggerOrderOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelFuturesTriggerOrderRest.CancelFuturesTriggerOrderAsync))
        {
        }
    }
}
