using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// A symbol representation based on a base and quote asset
    /// </summary>
    public class SharedSymbol
    {
        /// <summary>
        /// The base asset of the symbol
        /// </summary>
        public string BaseAsset { get; set; }
        /// <summary>
        /// The quote asset of the symbol
        /// </summary>
        public string QuoteAsset { get; set; }
        /// <summary>
        /// The symbol name, can be used to overwrite the default formatted name
        /// </summary>
        public string? SymbolName { get; set; }
        /// <summary>
        /// The trading mode of the symbol. This determines how the base and quote asset should be formatted into the symbol name
        /// </summary>
        public TradingMode TradingMode { get; set; }
        /// <summary>
        /// Delivery time of the symbol, used for delivery futures to format the symbol name
        /// </summary>
        public DateTime? DeliverTime { get; set; }

        /// <summary>
        /// Create a new SharedSymbol
        /// </summary>
        public SharedSymbol(TradingMode tradingMode, string baseAsset, string quoteAsset, DateTime? deliverTime = null)
        {
            TradingMode = tradingMode;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            DeliverTime = deliverTime;
        }

        /// <summary>
        /// Create a new SharedSymbol and override the formatted name
        /// </summary>
        public SharedSymbol(TradingMode tradingMode, string baseAsset, string quoteAsset, string symbolName)
        {
            TradingMode = tradingMode;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            SymbolName = symbolName;
        }

        /// <summary>
        /// Get the symbol name using the provided formatting function
        /// </summary>
        public string GetSymbol(Func<string, string, TradingMode, DateTime?, string> format)
        {
            if (!string.IsNullOrEmpty(SymbolName))
                return SymbolName!;

            return format(BaseAsset, QuoteAsset, TradingMode, DeliverTime);
        }
    }
}
