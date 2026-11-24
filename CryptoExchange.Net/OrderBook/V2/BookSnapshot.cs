using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.OrderBook.V2
{
    /// <summary>
    /// Book snapshot
    /// </summary>
    /// <param name="sequenceNumber">Sequence number</param>
    /// <param name="asks">Asks</param>
    /// <param name="bids">Bids</param>
    public class BookSnapshot(long sequenceNumber, IReadOnlyDictionary<decimal, decimal> asks, IReadOnlyDictionary<decimal, decimal> bids)
    {
        /// <summary>
        /// Sequence number
        /// </summary>
        public long SequenceNumber { get; } = sequenceNumber;
        /// <summary>
        /// Asks
        /// </summary>
        public IReadOnlyDictionary<decimal, decimal> Asks { get; } = asks;
        /// <summary>
        /// Bids
        /// </summary>
        public IReadOnlyDictionary<decimal, decimal> Bids { get; } = bids;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"   Ask quantity       Ask price | Bid price       Bid quantity");
            for (var i = 0; i < Math.Max(Asks.Count, Bids.Count); i++)
            {
                var askPrice = Asks.Count > i ? Asks.Keys.ElementAt(i) : (decimal?)null;
                var askQuantity = Asks.Count > i ? Asks.Values.ElementAt(i) : (decimal?)default;
                var bidPrice = Bids.Count > i ? Bids.Keys.ElementAt(i) : (decimal?)default;
                var bidQuantity = Bids.Count > i ? Bids.Values.ElementAt(i) : (decimal?)default;
                stringBuilder.AppendLine($"[{askQuantity?.ToString(CultureInfo.InvariantCulture),14}] {askPrice?.ToString(CultureInfo.InvariantCulture),14} | {bidPrice?.ToString(CultureInfo.InvariantCulture),-14} [{bidQuantity?.ToString(CultureInfo.InvariantCulture),-14}]");
            }

            return stringBuilder.ToString();
        }
    }

}
