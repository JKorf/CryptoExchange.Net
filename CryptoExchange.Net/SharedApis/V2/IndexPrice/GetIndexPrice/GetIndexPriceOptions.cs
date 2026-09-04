using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting the index price
    /// </summary>
    public class GetIndexPriceOptions : CapabilityOptions<GetIndexPriceRequest, IGetIndexPriceRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the index price for a futures symbol";

        /// <summary>
        /// ctor
        /// </summary>
        public GetIndexPriceOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetIndexPriceRest.GetIndexPriceAsync))
        {
        }
    }
}
