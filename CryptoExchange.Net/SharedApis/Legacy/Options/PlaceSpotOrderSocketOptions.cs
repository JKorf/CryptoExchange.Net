using CryptoExchange.Net.Objects;
using System;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for placing a new spot order
    /// </summary>
    public class PlaceSpotOrderSocketOptions : PlaceSpotOrderOptions
    {
        /// <inheritdoc />
        public override string Description => "Place a new spot order over a socket connection";


        /// <summary>
        /// ctor
        /// </summary>
        public PlaceSpotOrderSocketOptions(string exchange) : base(exchange)
        {
        }
    }
}
