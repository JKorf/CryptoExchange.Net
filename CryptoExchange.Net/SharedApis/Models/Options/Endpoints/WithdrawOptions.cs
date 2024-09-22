using CryptoExchange.Net.SharedApis.Models.Rest;

namespace CryptoExchange.Net.SharedApis.Models.Options.Endpoints
{
    /// <summary>
    /// Options for requesting a withdrawal
    /// </summary>
    public class WithdrawOptions : EndpointOptions<WithdrawRequest>
    {


        public WithdrawOptions() : base(true)
        {
        }

    }
}
