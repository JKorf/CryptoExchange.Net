using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting the mark price
    /// </summary>
    public class GetMarkPricesOptions : CapabilityOptions<GetMarkPricesRequest, IGetMarkPricesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the mark prices for all futures symbols";

        /// <summary>
        /// ctor
        /// </summary>
        public GetMarkPricesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetMarkPricesRest.GetMarkPricesAsync))
        {
        }
    }
}
