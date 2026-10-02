using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Order book info
    /// </summary>
    public record SharedIncrementalOrderBook : SharedOrderBook
    {
        /// <summary>
        /// The sequence number of the first book update in this update.
        /// </summary>
        public long? StartSequenceNumber { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedIncrementalOrderBook(SharedQuantityType quantityType, long? startSequenceNumber, long? endSequenceNumber, ISymbolOrderBookEntry[] asks, ISymbolOrderBookEntry[] bids)
            :base(quantityType, endSequenceNumber, asks, bids)
        {
            StartSequenceNumber = startSequenceNumber;
        }
    }

}
