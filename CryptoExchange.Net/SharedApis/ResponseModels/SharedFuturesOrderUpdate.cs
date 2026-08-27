using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Futures order info
    /// </summary>
    [DebuggerDisplay("{DebugView,nq}")]
    public record SharedFuturesOrderUpdate : SharedFuturesOrder
    {
        /// <summary>
        /// The info on the executed trade for this update
        /// </summary>
        public new SharedUserTrade? LastTrade
        {
#pragma warning disable CS0618 // Type or member is obsolete
            get => base.LastTrade;
            set => base.LastTrade = value;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFuturesOrderUpdate(
            SharedSymbol? sharedSymbol, 
            string symbol,
            string orderId,
            SharedOrderType orderType,
            SharedOrderSide orderSide,
            SharedOrderStatus orderStatus,
            DateTime? createTime)
            : base(sharedSymbol, symbol, orderId, orderType, orderSide, orderStatus, createTime)
        {
        }
    }
}
