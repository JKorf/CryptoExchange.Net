using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for closing position
    /// </summary>
    public class ClosePositionOptions : CapabilityOptions<ClosePositionRequest, IClosePositionEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Close an open futures position";

        /// <summary>
        /// ctor
        /// </summary>
        public ClosePositionOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IClosePositionEndpoint.ClosePositionAsync))
        {
        }
    }
}
