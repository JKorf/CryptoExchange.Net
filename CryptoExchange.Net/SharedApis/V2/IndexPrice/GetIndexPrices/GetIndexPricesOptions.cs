using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting the index price
    /// </summary>
    public class GetIndexPricesOptions : CapabilityOptions<GetIndexPricesRequest, IGetIndexPricesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the index prices for all futures symbols";

        /// <summary>
        /// ctor
        /// </summary>
        public GetIndexPricesOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetIndexPricesRest.GetIndexPricesAsync))
        {
        }
    }
}
