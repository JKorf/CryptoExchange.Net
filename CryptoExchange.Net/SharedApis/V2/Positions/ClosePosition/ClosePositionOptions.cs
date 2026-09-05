using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for closing position
    /// </summary>
    public class ClosePositionOptions : CapabilityOptions<ClosePositionRequest, IClosePositionRest>
    {
        /// <inheritdoc />
        public override string Description => "Close an open futures position";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<ClosePositionRequest>.Required(x => x.Symbol, "The symbol of the position to close", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<ClosePositionRequest>.Required(x => x.PositionMode, "The current position mode of the account", SharedPositionMode.OneWay),
            RequestParameterRule<ClosePositionRequest>.Optional(x => x.PositionSide, "The side of the position to close", SharedPositionSide.Long),
            RequestParameterRule<ClosePositionRequest>.Optional(x => x.MarginMode, "The margin mode of the position", SharedMarginMode.Cross),
            RequestParameterRule<ClosePositionRequest>.Optional(x => x.Quantity, "The quantity of the position to close", 1m),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public ClosePositionOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IClosePositionRest.ClosePositionAsync), _defaultParameterRules)
        {
        }
    }
}
