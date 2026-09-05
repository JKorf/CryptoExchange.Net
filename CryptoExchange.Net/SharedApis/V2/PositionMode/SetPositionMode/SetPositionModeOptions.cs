namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting position mode
    /// </summary>
    public class SetPositionModeOptions : CapabilityOptions<SetPositionModeRequest, ISetPositionModeRest>
    {
        /// <inheritdoc />
        public override string Description => "Set the futures position mode";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SetPositionModeRequest>.Optional(x => x.TradingMode, "The trading mode to set the position mode for", TradingMode.PerpetualLinear),
            RequestParameterRule<SetPositionModeRequest>.Optional(x => x.Symbol, "The symbol to set the position mode for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<SetPositionModeRequest>.Required(x => x.PositionMode, "The position mode to set", SharedPositionMode.OneWay),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SetPositionModeOptions(string exchange) : base(exchange, true, nameof(ISetPositionModeRest.SetPositionModeOptions), _defaultParameterRules)
        {
        }
    }
}
