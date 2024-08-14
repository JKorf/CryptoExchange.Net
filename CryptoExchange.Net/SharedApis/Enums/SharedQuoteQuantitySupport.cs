using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Enums
{
    /// <summary>
    /// Quote asset order quantity support
    /// </summary>
    public enum SharedQuantityType
    {
        BaseAssetQuantity,
        QuoteAssetQuantity,
        Both
    }

    public record SharedQuantitySupport
    {
        public SharedQuantityType BuyLimit { get; }
        public SharedQuantityType SellLimit { get; }
        public SharedQuantityType BuyMarket { get; }
        public SharedQuantityType SellMarket { get; }

        public SharedQuantitySupport(SharedQuantityType buyLimit, SharedQuantityType sellLimit, SharedQuantityType buyMarket, SharedQuantityType sellMarket)
        {
            BuyLimit = buyLimit;
            SellLimit = sellLimit;
            BuyMarket = buyMarket;
            SellMarket = sellMarket;
        }

        public SharedQuantityType GetSupportedQuantityType(SharedOrderSide side, SharedOrderType orderType)
        {
            if (side == SharedOrderSide.Buy && (orderType == SharedOrderType.Limit || orderType == SharedOrderType.LimitMaker)) return BuyLimit;
            if (side == SharedOrderSide.Buy && orderType == SharedOrderType.Market) return BuyMarket;
            if (side == SharedOrderSide.Sell && (orderType == SharedOrderType.Limit || orderType == SharedOrderType.LimitMaker)) return SellLimit;
            if (side == SharedOrderSide.Sell && orderType == SharedOrderType.Market) return SellMarket;

            throw new ArgumentException("Unknown side/type combination");
        }
    }
}
