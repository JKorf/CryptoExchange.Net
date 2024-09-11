using CryptoExchange.Net.Objects;
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

        public Error? Validate(SharedOrderSide side, SharedOrderType type, decimal? quantity, decimal? quoteQuantity)
        {
            var supportedType = GetSupportedQuantityType(side, type);
            if (supportedType == SharedQuantityType.Both)
                return null;

            if (supportedType == SharedQuantityType.BaseAssetQuantity && quoteQuantity != null)
                return new ArgumentError($"Quote quantity not supported for {side}.{type} order, specify Quantity instead");

            if (supportedType == SharedQuantityType.QuoteAssetQuantity && quantity != null)
                return new ArgumentError($"Quantity not supported for {side}.{type} order, specify QuoteQuantity instead");

            return null;
        }

        public override string ToString()
        {
            return $"Limit buy: {BuyLimit}, limit sell: {SellLimit}, market buy: {BuyMarket}, market sell: {SellMarket}";
        }
    }
}
