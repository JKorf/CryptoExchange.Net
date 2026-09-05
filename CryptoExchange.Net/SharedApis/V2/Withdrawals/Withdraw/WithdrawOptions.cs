namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting a withdrawal
    /// </summary>
    public class WithdrawOptions : CapabilityOptions<WithdrawRequest, IWithdrawRest>
    {
        /// <inheritdoc />
        public override string Description => "Withdraw an asset";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<WithdrawRequest>.Required(x => x.Asset, "The asset to withdraw", "ETH"),
            RequestParameterRule<WithdrawRequest>.Required(x => x.Address, "The address to withdraw to", "0x123"),
            RequestParameterRule<WithdrawRequest>.Required(x => x.Quantity, "The quantity to withdraw", 1m),
            RequestParameterRule<WithdrawRequest>.Optional(x => x.AddressTag, "The address tag or memo", "123"),
            RequestParameterRule<WithdrawRequest>.Optional(x => x.Network, "The network to use for the withdrawal", "ERC20"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public WithdrawOptions(string exchange) : base(exchange, true, nameof(IWithdrawRest.WithdrawAsync), _defaultParameterRules)
        {
        }
    }
}
