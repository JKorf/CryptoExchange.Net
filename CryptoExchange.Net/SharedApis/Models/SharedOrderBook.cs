using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Order book info
    /// </summary>
    public record SharedOrderBook
    {
        /// <summary>
        /// The quantity notation the order book is in, either BaseAsset or Contract
        /// </summary>
        public SharedQuantityType QuantityType { get; set; }
        /// <summary>
        /// The sequence number this snapshot or update is at
        /// </summary>
        public long? SequenceNumber { get; set; }
        /// <summary>
        /// Asks list
        /// </summary>
        public ISymbolOrderBookEntry[] Asks { get; set; }
        /// <summary>
        /// Bids list
        /// </summary>
        public ISymbolOrderBookEntry[] Bids { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedOrderBook(SharedQuantityType quantityType, long? sequenceNumber, ISymbolOrderBookEntry[] asks, ISymbolOrderBookEntry[] bids)
        {
            QuantityType = quantityType;
            SequenceNumber = sequenceNumber;
            Asks = asks;
            Bids = bids;
        }
    }

}
