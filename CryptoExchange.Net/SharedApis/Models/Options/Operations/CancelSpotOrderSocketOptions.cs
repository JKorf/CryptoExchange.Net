using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a spot order
    /// </summary>
    public class CancelSpotOrderSocketOptions : CancelSpotOrderOptions
    {
        /// <inheritdoc />
        public override string Description => "Cancel a spot order over a socket connection";

        /// <summary>
        /// ctor
        /// </summary>
        public CancelSpotOrderSocketOptions(string exchange, bool authenticated) : base(exchange, authenticated)
        {
        }
    }
}
