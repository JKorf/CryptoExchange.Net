using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting trading fee info
    /// </summary>
    public class GetFeeOptions : CapabilityOptions<GetFeeRequest, IGetFeesRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve trading fee information";

        /// <summary>
        /// ctor
        /// </summary>
        public GetFeeOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IGetFeesRest.GetFeesAsync))
        {
        }
    }
}
