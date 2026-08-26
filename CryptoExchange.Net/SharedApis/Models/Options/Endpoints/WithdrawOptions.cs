namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a withdrawal
    /// </summary>
    public class WithdrawOptions : CapabilityOptions<WithdrawRequest, IWithdrawEndpoint>
    {
        /// <inheritdoc />
        public override string Description => "Withdraw an asset";

        /// <summary>
        /// ctor
        /// </summary>
        public WithdrawOptions(string exchange) : base(exchange, true, nameof(IWithdrawEndpoint.WithdrawAsync))
        {
        }
    }
}
