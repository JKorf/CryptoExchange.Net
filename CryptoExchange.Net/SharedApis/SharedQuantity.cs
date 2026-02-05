using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Quantity reference
    /// </summary>
    public record SharedQuantityReference
    {
        /// <summary>
        /// Quantity denoted in the base asset of the symbol
        /// </summary>
        public decimal? QuantityInBaseAsset { get; set; }
        /// <summary>
        /// Quantity denoted in the quote asset of the symbol
        /// </summary>
        public decimal? QuantityInQuoteAsset { get; set; }
        /// <summary>
        /// Quantity denoted in the number of contracts
        /// </summary>
        public decimal? QuantityInContracts { get; set; }

        /// <summary>
        /// Whether all values are null or zero
        /// </summary>
        public bool IsZero => !(QuantityInBaseAsset > 0) && !(QuantityInQuoteAsset > 0) && !(QuantityInContracts > 0);

        /// <summary>
        /// ctor
        /// </summary>
        internal SharedQuantityReference(decimal? baseAssetQuantity, decimal? quoteAssetQuantity, decimal? contractQuantity)
        {
            QuantityInBaseAsset = baseAssetQuantity;
            QuantityInQuoteAsset = quoteAssetQuantity;
            QuantityInContracts = contractQuantity;
        }
    }

    /// <summary>
    /// Quantity for an order
    /// </summary>
    [JsonConverter(typeof(SharedQuantityConverter))]
    public record SharedQuantity : SharedQuantityReference
    {
        private SharedQuantity(decimal? baseAssetQuantity, decimal? quoteAssetQuantity, decimal? contractQuantity)
            : base(baseAssetQuantity, quoteAssetQuantity, contractQuantity)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedQuantity() : base(null, null, null) { }

        /// <summary>
        /// Specify quantity in base asset
        /// </summary>
        public static SharedQuantity Base(decimal quantity) => new SharedQuantity(quantity, null, null);
        /// <summary>
        /// Specify quantity in quote asset
        /// </summary>
        public static SharedQuantity Quote(decimal quantity) => new SharedQuantity(null, quantity, null);
        /// <summary>
        /// Specify quantity in number of contracts
        /// </summary>
        public static SharedQuantity Contracts(decimal quantity) => new SharedQuantity(null, null, quantity);

        /// <summary>
        /// Get the base asset quantity from a quote quantity using a price
        /// </summary>
        /// <param name="quoteQuantity">Quantity in quote asset to convert</param>
        /// <param name="price">Price to use for conversion</param>
        /// <param name="decimalPlaces">The max number of decimal places for the result</param>
        /// <param name="lotSize">The lot size (step per quantity) for the base asset</param>
        public static SharedQuantity BaseFromQuote(decimal quoteQuantity, decimal price, int decimalPlaces = 8, decimal lotSize = 0.00000001m) 
            => new SharedQuantity(ExchangeHelpers.ApplyRules(quoteQuantity / price, decimalPlaces, lotSize), null, null);
        /// <summary>
        /// Get the quote asset quantity from a base quantity using a price
        /// </summary>
        /// <param name="baseQuantity">Quantity in base asset to convert</param>
        /// <param name="price">Price to use for conversion</param>
        /// <param name="decimalPlaces">The max number of decimal places for the result</param>
        /// <param name="lotSize">The lot size (step per quantity) for the quote asset</param>
        public static SharedQuantity QuoteFromBase(decimal baseQuantity, decimal price, int decimalPlaces = 8, decimal lotSize = 0.00000001m) 
            => new SharedQuantity(ExchangeHelpers.ApplyRules(baseQuantity * price, decimalPlaces, lotSize), null, null);
        /// <summary>
        /// Get a quantity in number of contracts from a base asset
        /// </summary>
        /// <param name="baseQuantity">Quantity in base asset to convert</param>
        /// <param name="contractSize">The contract size of a single contract</param>
        /// <param name="decimalPlaces">The max number of decimal places for the result</param>
        /// <param name="lotSize">The lot size (step per quantity) for the contract</param>
        public static SharedQuantity ContractsFromBase(decimal baseQuantity, decimal contractSize, int decimalPlaces = 8, decimal lotSize = 0.00000001m)
            => new SharedQuantity(ExchangeHelpers.ApplyRules(baseQuantity / contractSize, decimalPlaces, lotSize), null, null);
        /// <summary>
        /// Get a quantity in number of contracts from a quote asset
        /// </summary>
        /// <param name="quoteQuantity">Quantity in quote asset to convert</param>
        /// <param name="contractSize">The contract size of a single contract</param>
        /// <param name="price">The price to use for conversion</param>
        /// <param name="decimalPlaces">The max number of decimal places for the result</param>
        /// <param name="lotSize">The lot size (step per quantity) for the contract</param>
        public static SharedQuantity ContractsFromQuote(decimal quoteQuantity, decimal contractSize, decimal price, int decimalPlaces = 8, decimal lotSize = 0.00000001m) 
            => new SharedQuantity(ExchangeHelpers.ApplyRules(quoteQuantity / price / contractSize, decimalPlaces, lotSize), null, null);
    }

    /// <summary>
    /// Order quantity
    /// </summary>
    [JsonConverter(typeof(SharedOrderQuantityConverter))]
    public record SharedOrderQuantity : SharedQuantityReference
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SharedOrderQuantity(): base(null, null,null) { }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedOrderQuantity(decimal? baseAssetQuantity = null, decimal? quoteAssetQuantity = null, decimal? contractQuantity = null)
            : base(baseAssetQuantity, quoteAssetQuantity, contractQuantity)
        {
        }
    }
}
