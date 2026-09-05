namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting leverage
    /// </summary>
    public class SetLeverageOptions : CapabilityOptions<SetLeverageRequest, ISetLeverageRest>
    {
        /// <inheritdoc />
        public override string Description => "Set the leverage for a futures symbol";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SetLeverageRequest>.Required(x => x.Symbol, "The symbol to set leverage for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<SetLeverageRequest>.Required(x => x.Leverage, "The leverage to set", 10m),
            RequestParameterRule<SetLeverageRequest>.Optional(x => x.Side, "The position side to set leverage for", SharedPositionSide.Long),
            RequestParameterRule<SetLeverageRequest>.Optional(x => x.MarginMode, "The margin mode to set leverage for", SharedMarginMode.Cross),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SetLeverageOptions(string exchange) : base(exchange, true, nameof(ISetLeverageRest.SetLeverageAsync), _defaultParameterRules)
        {
        }
    }
}
