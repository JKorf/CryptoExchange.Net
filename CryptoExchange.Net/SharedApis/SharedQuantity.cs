using System;
using System.Collections.Generic;
using System.Text;

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
        /// ctor
        /// </summary>
        protected SharedQuantityReference(decimal? baseAssetQuantity, decimal? quoteAssetQuantity, decimal? contractQuantity)
        {
            QuantityInBaseAsset = baseAssetQuantity;
            QuantityInQuoteAsset = quoteAssetQuantity;
            QuantityInContracts = contractQuantity;
        }
    }

    /// <summary>
    /// Quantity for an order
    /// </summary>
    public record SharedQuantity : SharedQuantityReference
    {
        private SharedQuantity(decimal? baseAssetQuantity, decimal? quoteAssetQuantity, decimal? contractQuantity)
            : base(baseAssetQuantity, quoteAssetQuantity, contractQuantity)
        {
        }

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
        public static SharedQuantity BaseFromQuote(decimal quoteQuantity, decimal price) => new SharedQuantity(Math.Round(quoteQuantity / price, 8), null, null);
        /// <summary>
        /// Get the quote asset quantity from a base quantity using a price
        /// </summary>
        /// <param name="baseQuantity">Quantity in base asset to convert</param>
        /// <param name="price">Price to use for conversion</param>
        public static SharedQuantity QuoteFromBase(decimal baseQuantity, decimal price) => new SharedQuantity(Math.Round(baseQuantity * price, 8), null, null);
        /// <summary>
        /// Get a quantity in number of contracts from a base asset
        /// </summary>
        /// <param name="baseQuantity">Quantity in base asset to convert</param>
        /// <param name="contractSize">The contract size of a single contract</param>
        /// <returns></returns>
        public static SharedQuantity ContractsFromBase(decimal baseQuantity, decimal contractSize) => new SharedQuantity(Math.Round(baseQuantity / contractSize, 8), null, null);
        /// <summary>
        /// Get a quantity in number of contracts from a quote asset
        /// </summary>
        /// <param name="quoteQuantity">Quantity in quote asset to convert</param>
        /// <param name="contractSize">The contract size of a single contract</param>
        /// <param name="price">The price to use for conversion</param>
        /// <returns></returns>
        public static SharedQuantity ContractsFromQuote(decimal quoteQuantity, decimal contractSize, decimal price) => new SharedQuantity(Math.Round(quoteQuantity / price / contractSize, 8), null, null);
    }

    /// <summary>
    /// Order quantity
    /// </summary>
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
