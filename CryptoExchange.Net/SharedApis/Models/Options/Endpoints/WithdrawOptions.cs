namespace CryptoExchange.Net.SharedApis
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
