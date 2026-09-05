namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for requesting current position mode
    /// </summary>
    public class GetPositionModeOptions : CapabilityOptions<GetPositionModeRequest, IGetPositionModeRest>
    {
        /// <inheritdoc />
        public override string Description => "Retrieve the current futures position mode";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<GetPositionModeRequest>.Optional(x => x.TradingMode, "The trading mode to retrieve the position mode for", TradingMode.PerpetualLinear),
            RequestParameterRule<GetPositionModeRequest>.Optional(x => x.Symbol, "The symbol to retrieve the position mode for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public GetPositionModeOptions(string exchange) : base(exchange, true, nameof(IGetPositionModeRest.GetPositionModeAsync), _defaultParameterRules)
        {
        }
    }
}
