using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for canceling a TP/SL
    /// </summary>
    public class CancelFuturesTpSlOptions : CapabilityOptions<CancelTpSlRequest, ICancelFuturesTpSlRest>
    {
        /// <inheritdoc />
        public override string Description => "Cancel a take profit or stop loss order for a futures position";

        private static readonly RequestParameterDescription[] _defaultParameterRules = new[]
        {
            RequestParameterRule<CancelTpSlRequest>.Required(x => x.Symbol, "The symbol of the take profit or stop loss order to cancel", new SharedSymbol(TradingMode.PerpetualLinear, "ETH", "USDT")),
            RequestParameterRule<CancelTpSlRequest>.Optional(x => x.OrderId, "The id of the take profit or stop loss order to cancel", "123"),
            RequestParameterRule<CancelTpSlRequest>.Optional(x => x.PositionMode, "The position mode of the account", SharedPositionMode.OneWay),
            RequestParameterRule<CancelTpSlRequest>.Optional(x => x.PositionSide, "The side of the position", SharedPositionSide.Long),
            RequestParameterRule<CancelTpSlRequest>.Optional(x => x.TpSlSide, "The take profit or stop loss side to cancel", SharedTpSlSide.TakeProfit),
            RequestParameterRule<CancelTpSlRequest>.Optional(x => x.MarginMode, "The margin mode of the position", SharedMarginMode.Cross),
        };

        /// <summary>
        /// ctor
        /// </summary>
        public CancelFuturesTpSlOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(ICancelFuturesTpSlRest.CancelFuturesTpSlAsync), _defaultParameterRules)
        {
        }
    }
}
