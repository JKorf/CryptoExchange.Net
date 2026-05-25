namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a withdrawal
    /// </summary>
    public class WithdrawOptions : EndpointOptions<WithdrawRequest, IWithdrawRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public WithdrawOptions(string exchange) : base(exchange, true)
        {
        }
    }
}
