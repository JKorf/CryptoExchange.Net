using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a TP/SL
    /// </summary>
    public class CancelFuturesTpSlOptions : CapabilityOptions<CancelTpSlRequest, ICancelFuturesTpSlRest>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a take profit or stop loss order for a futures position";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesTpSlOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelFuturesTpSlRest.CancelFuturesTpSlAsync))
        {
        }
    }
}
