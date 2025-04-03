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
        public SharedQuantityType BuyLimit { get; set; }
        /// <summary>
        /// Supported quantity notations for sell limit orders
        /// </summary>
        public SharedQuantityType SellLimit { get; set; }
        /// <summary>
        /// Supported quantity notations for buy market orders
        /// </summary>
        public SharedQuantityType BuyMarket { get; set; }
        /// <summary>
        /// Supported quantity notations for sell market orders
        /// </summary>
        public SharedQuantityType SellMarket { get; set; }

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
        public Error? Validate(SharedOrderSide side, SharedOrderType type, SharedQuantity? quantity)
        {
            var supportedType = GetSupportedQuantityType(side, type);
            if (supportedType == SharedQuantityType.BaseAndQuoteAsset)
                return null;

            if (supportedType == SharedQuantityType.BaseAndQuoteAsset && quantity != null && quantity.QuantityInBaseAsset == null && quantity.QuantityInQuoteAsset == null)
                return new ArgumentError($"Quantity for {side}.{type} required in base or quote asset");

            if (supportedType == SharedQuantityType.QuoteAsset && quantity != null && quantity.QuantityInQuoteAsset == null)
                return new ArgumentError($"Quantity for {side}.{type} required in quote asset");

            if (supportedType == SharedQuantityType.BaseAsset && quantity != null && quantity.QuantityInBaseAsset == null && quantity.QuantityInContracts == null)
                return new ArgumentError($"Quantity for {side}.{type} required in base asset");

            if (supportedType == SharedQuantityType.Contracts && quantity != null && quantity.QuantityInContracts == null)
                return new ArgumentError($"Quantity for {side}.{type} required in contracts");

            return null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Limit buy: {BuyLimit}, limit sell: {SellLimit}, market buy: {BuyMarket}, market sell: {SellMarket}";
        }
    }
}
