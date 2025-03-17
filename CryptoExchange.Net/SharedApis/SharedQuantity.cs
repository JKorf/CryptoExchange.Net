using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public record SharedQuantityReference
    {
        public decimal? QuantityInBaseAsset { get; set; }
        public decimal? QuantityInQuoteAsset { get; set; }
        public decimal? QuantityInContracts { get; set; }

        protected SharedQuantityReference(decimal? baseAssetQuantity, decimal? quoteAssetQuantity, decimal? contractQuantity)
        {
            QuantityInBaseAsset = baseAssetQuantity;
            QuantityInQuoteAsset = quoteAssetQuantity;
            QuantityInContracts = contractQuantity;
        }
    }

    public record SharedQuantity : SharedQuantityReference
    {
        private SharedQuantity(decimal? baseAssetQuantity, decimal? quoteAssetQuantity, decimal? contractQuantity)
            : base(baseAssetQuantity, quoteAssetQuantity, contractQuantity)
        {
        }

        public static SharedQuantity Base(decimal quantity) => new SharedQuantity(quantity, null, null);
        public static SharedQuantity Quote(decimal quantity) => new SharedQuantity(null, quantity, null);
        public static SharedQuantity Contracts(decimal quantity) => new SharedQuantity(null, null, quantity);

        public static SharedQuantity BaseFromQuote(decimal quoteQuantity, decimal price) => new SharedQuantity(Math.Round(quoteQuantity / price, 8), null, null);
        public static SharedQuantity QuoteFromBase(decimal baseQuantity, decimal price) => new SharedQuantity(Math.Round(baseQuantity * price, 8), null, null);
        public static SharedQuantity ContractsFromBase(decimal baseQuantity, decimal contractSize) => new SharedQuantity(Math.Round(baseQuantity / contractSize, 8), null, null);
        public static SharedQuantity ContractsFromQuote(decimal quoteQuantity, decimal contractSize, decimal price) => new SharedQuantity(Math.Round(quoteQuantity / price / contractSize, 8), null, null);
    }

    public record SharedOrderQuantity : SharedQuantityReference
    {
        public SharedOrderQuantity(): base(null, null,null) { }

        public SharedOrderQuantity(decimal? baseAssetQuantity, decimal? quoteAssetQuantity, decimal? contractQuantity)
            : base(baseAssetQuantity, quoteAssetQuantity, contractQuantity)
        {
        }
    }
}
