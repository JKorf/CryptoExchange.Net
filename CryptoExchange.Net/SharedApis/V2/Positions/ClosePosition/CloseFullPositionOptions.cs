using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for fully closing a position
    /// </summary>
    public class CloseFullPositionOptions : CapabilityOptions<CloseFullPositionRequest, ICloseFullPosition>
    {
        /// <inheritdoc />
        public override string Description => "Fully close an open futures position";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<CloseFullPositionRequest>.Required(x => x.Symbol, "The symbol of the position to close", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<CloseFullPositionRequest>.Optional(x => x.PositionMode, "The current position mode of the account", SharedPositionMode.OneWay),
            RequestParameterRule<CloseFullPositionRequest>.Optional(x => x.PositionSide, "The side of the position to close", SharedPositionSide.Long),
            RequestParameterRule<CloseFullPositionRequest>.Optional(x => x.MarginMode, "The margin mode of the position", SharedMarginMode.Cross),
            RequestParameterRule<CloseFullPositionRequest>.Optional(x => x.PositionId, "The id of the position to close", "12345"),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public CloseFullPositionOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICloseFullPosition.CloseFullPositionAsync), _defaultParameterRules)
        {
        }
    }
}
