using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Options.Endpoints
{
    /// <summary>
    /// Options for setting position mode
    /// </summary>
    public class SetPositionModeOptions : EndpointOptions<SetPositionModeRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SetPositionModeOptions() : base(true)
        {
        }
    }
}
