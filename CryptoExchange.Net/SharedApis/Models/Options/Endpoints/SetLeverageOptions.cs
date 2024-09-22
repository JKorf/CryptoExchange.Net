using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Options.Endpoints
{
    /// <summary>
    /// Options for setting leverage
    /// </summary>
    public class SetLeverageOptions : EndpointOptions<SetLeverageRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SetLeverageOptions() : base(true)
        {
        }
    }
}
