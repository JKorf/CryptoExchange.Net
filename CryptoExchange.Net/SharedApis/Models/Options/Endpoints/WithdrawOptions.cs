namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a withdrawal
    /// </summary>
    public class WithdrawOptions : EndpointOptions<WithdrawRequest>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public WithdrawOptions() : base(true)
        {
        }
    }
}
