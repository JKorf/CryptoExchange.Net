using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Support for different quantity notations
    /// </summary>
    public record SharedQuantitySupport
    {
        /// <summary>
        /// Supported quantity notations for buy limit orders
        /// </summary>
        public SharedQuantityType BuyLimit { get; }
        /// <summary>
        /// Supported quantity notations for sell limit orders
        /// </summary>
        public SharedQuantityType SellLimit { get; }
        /// <summary>
        /// Supported quantity notations for buy market orders
        /// </summary>
        public SharedQuantityType BuyMarket { get; }
        /// <summary>
        /// Supported quantity notations for sell market orders
        /// </summary>
        public SharedQuantityType SellMarket { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedQuantitySupport(SharedQuantityType buyLimit, SharedQuantityType sellLimit, SharedQuantityType buyMarket, SharedQuantityType sellMarket)
        {
            BuyLimit = buyLimit;
            SellLimit = sellLimit;
            BuyMarket = buyMarket;
            SellMarket = sellMarket;
        }

        private SharedQuantityType GetSupportedQuantityType(SharedOrderSide side, SharedOrderType orderType)
        {
            if (side == SharedOrderSide.Buy && (orderType == SharedOrderType.Limit || orderType == SharedOrderType.LimitMaker)) return BuyLimit;
            if (side == SharedOrderSide.Buy && orderType == SharedOrderType.Market) return BuyMarket;
            if (side == SharedOrderSide.Sell && (orderType == SharedOrderType.Limit || orderType == SharedOrderType.LimitMaker)) return SellLimit;
            if (side == SharedOrderSide.Sell && orderType == SharedOrderType.Market) return SellMarket;

            throw new ArgumentException("Unknown side/type combination");
        }

        /// <summary>
        /// Validate a request
        /// </summary>
        /// <param name="side"></param>
        /// <param name="type"></param>
        /// <param name="quantity"></param>
        /// <param name="quoteQuantity"></param>
        /// <returns></returns>
        public Error? Validate(SharedOrderSide side, SharedOrderType type, decimal? quantity, decimal? quoteQuantity)
        {
            var supportedType = GetSupportedQuantityType(side, type);
            if (supportedType == SharedQuantityType.BaseAndQuoteAsset)
                return null;

            if ((supportedType == SharedQuantityType.BaseAsset || supportedType == SharedQuantityType.Contracts) && quoteQuantity != null)
                return new ArgumentError($"Quote quantity not supported for {side}.{type} order, specify Quantity instead");

            if (supportedType == SharedQuantityType.QuoteAsset && quantity != null)
                return new ArgumentError($"Quantity not supported for {side}.{type} order, specify QuoteQuantity instead");

            return null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Limit buy: {BuyLimit}, limit sell: {SellLimit}, market buy: {BuyMarket}, market sell: {SellMarket}";
        }
    }
}
