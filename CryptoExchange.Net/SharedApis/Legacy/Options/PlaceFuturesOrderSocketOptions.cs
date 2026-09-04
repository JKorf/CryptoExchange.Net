using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new futures order
    /// </summary>
    public class PlaceFuturesOrderSocketOptions : PlaceFuturesOrderOptions
    {
        /// <inheritdoc />
        public override string Description => "Place a new futures order over a socket connection";

        /// <summary>
        /// ctor
        /// </summary>
        public PlaceFuturesOrderSocketOptions(string exchange, bool supportsTpSl) : base(exchange, supportsTpSl)
        {
        }

    }
}
