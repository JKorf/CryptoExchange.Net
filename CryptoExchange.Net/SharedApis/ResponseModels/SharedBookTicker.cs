using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Book ticker
    /// </summary>
    [DebuggerDisplay("{Symbol,nq} - {BestBidPrice} / {BestAskPrice}")]
    public record SharedBookTicker : SharedSymbolModel
    {
        /// <summary>
        /// Price of the best ask
        /// </summary>
        public decimal BestAskPrice { get; set; }
        /// <summary>
        /// Quantity of the best ask
        /// </summary>
        [Obsolete("Use `BestAskQuantities` instead")]
        public decimal BestAskQuantity => BestAskQuantities.QuantityInBaseAsset ?? BestAskQuantities.QuantityInContracts ?? 0;
        /// <summary>
        /// Quantities of the best ask
        /// </summary>
        public SharedOrderQuantity BestAskQuantities { get; set; }

        /// <summary>
        /// Price of the best bid
        /// </summary>
        public decimal BestBidPrice { get; set; }

        /// <summary>
        /// Quantity of the best bid
        /// </summary>
        [Obsolete("Use `BestBidQuantities` instead")]
        public decimal BestBidQuantity => BestBidQuantities.QuantityInBaseAsset ?? BestBidQuantities.QuantityInContracts ?? 0;
        /// <summary>
        /// Quantities of the best bid
        /// </summary>
        public SharedOrderQuantity BestBidQuantities { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedBookTicker(
            SharedSymbol? sharedSymbol,
            string symbol,
            decimal bestAskPrice,
            SharedOrderQuantity bestAskQuantity,
            decimal bestBidPrice,
            SharedOrderQuantity bestBidQuantity)
            : base(sharedSymbol, symbol)
        {
            BestAskPrice = bestAskPrice;
            BestAskQuantities = bestAskQuantity.WithCalculatedQuantities(bestAskPrice, null);
            BestBidPrice = bestBidPrice;
            BestBidQuantities = bestBidQuantity.WithCalculatedQuantities(bestBidPrice, null);
        }
    }
}
