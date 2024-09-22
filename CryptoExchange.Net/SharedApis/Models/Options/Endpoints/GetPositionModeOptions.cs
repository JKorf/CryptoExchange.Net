using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Options.Endpoints
{
    /// <summary>
    /// Options for requesting current position mode
    /// </summary>
    public class GetPositionModeOptions : EndpointOptions<GetPositionModeRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionModeOptions() : base(true)
        {
        }
    }
}
