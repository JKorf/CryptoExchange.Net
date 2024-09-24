using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Futures symbol info
    /// </summary>
    public record SharedFuturesSymbol : SharedSpotSymbol
    {
        /// <summary>
        /// Symbol type
        /// </summary>
        public SharedSymbolType SymbolType { get; set; }
        /// <summary>
        /// The size of a single contract
        /// </summary>
        public decimal? ContractSize { get; set; }
        /// <summary>
        /// Delivery time of the contract
        /// </summary>
        public DateTime? DeliveryTime { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFuturesSymbol(SharedSymbolType symbolType, string baseAsset, string quoteAsset, string symbol, bool trading) : base(baseAsset, quoteAsset, symbol, trading)
        {
            SymbolType = symbolType;
        }
    }
}
