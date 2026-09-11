using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a futures order
    /// </summary>
    public class CancelFuturesOrderSocketOptions : CancelFuturesOrderOptions
    {
        /// <inheritdoc />
        public override string Description => "Cancel a futures order over a socket connection";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesOrderSocketOptions(string exchange, bool authenticated) : base(exchange, authenticated)
        {
        }
    }
}
