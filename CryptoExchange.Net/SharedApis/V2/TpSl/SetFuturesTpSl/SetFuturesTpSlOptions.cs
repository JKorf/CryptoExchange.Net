using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for setting a TP/SL
    /// </summary>
    public class SetFuturesTpSlOptions : CapabilityOptions<SetTpSlRequest, ISetFuturesTpSlRest>
    {
        /// <inheritdoc />
        public override string Description => "Set take profit or stop loss values for a futures position";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<SetTpSlRequest>.Required(x => x.Symbol, "The symbol to set take profit or stop loss for", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<SetTpSlRequest>.Optional(x => x.PositionMode, "The position mode of the account", SharedPositionMode.OneWay),
            RequestParameterRule<SetTpSlRequest>.Required(x => x.PositionSide, "The side of the position", SharedPositionSide.Long),
            RequestParameterRule<SetTpSlRequest>.Optional(x => x.MarginMode, "The margin mode of the position", SharedMarginMode.Cross),
            RequestParameterRule<SetTpSlRequest>.Required(x => x.TpSlSide, "Whether to set take profit or stop loss", SharedTpSlSide.TakeProfit),
            RequestParameterRule<SetTpSlRequest>.Optional(x => x.Quantity, "The quantity to close", 1m),
            RequestParameterRule<SetTpSlRequest>.Required(x => x.TriggerPrice, "The trigger price", 1m),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public SetFuturesTpSlOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ISetFuturesTpSlRest.SetFuturesTpSlAsync), _defaultParameterRules)
        {
        }
    }
}
