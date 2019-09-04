using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

namespace CryptoExchange.Net.OrderBook
{
    /// <summary>
    /// Base for order book implementations
    /// </summary>
    public abstract class SymbolOrderBook : IDisposable
    {
        /// <summary>
        /// The process buffer, used while syncing
        /// </summary>
        protected readonly List<ProcessBufferEntry> processBuffer;
        private readonly object bookLock = new object();
        /// <summary>
        /// The ask list
        /// </summary>
        protected SortedList<decimal, OrderBookEntry> asks;
        /// <summary>
        /// The bid list
        /// </summary>

        protected SortedList<decimal, OrderBookEntry> bids;
        private OrderBookStatus status;
        private UpdateSubscription subscription;
        private readonly bool sequencesAreConsecutive;
        private readonly string id;
        /// <summary>
        /// The log
        /// </summary>
        protected Log log;

        private bool bookSet;

        /// <summary>
        /// The status of the order book. Order book is up to date when the status is `Synced`
        /// </summary>
        public OrderBookStatus Status
        {
            get => status;
            set
            {
                if (value == status)
                    return;

                var old = status;
                status = value;
                log.Write(LogVerbosity.Info, $"{id} order book {Symbol} status changed: {old} => {value}");
                OnStatusChange?.Invoke(old, status);
            }
        }

        /// <summary>
        /// Last update identifier
        /// </summary>
        public long LastSequenceNumber { get; private set; }
        /// <summary>
        /// The symbol of the order book
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Event when the state changes
        /// </summary>
        public event Action<OrderBookStatus, OrderBookStatus> OnStatusChange;
        /// <summary>
        /// Event when orderbook was updated. Be careful! It can generate a lot of events at high-liquidity markets
        /// </summary>    
        public event Action OnOrderBookUpdate;
        /// <summary>
        /// Should be useful for low-liquidity order-books to monitor market activity
        /// </summary>
        public DateTime LastOrderBookUpdate;

        /// <summary>
        /// The number of asks in the book
        /// </summary>
        public int AskCount { get; private set; }
        /// <summary>
        /// The number of bids in the book
        /// </summary>
        public int BidCount { get; private set; }

        /// <summary>
        /// The list of asks
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Asks
        {
            get
            {
                lock (bookLock)
                    return asks.Select(a => a.Value).ToList();
            }
        }

        /// <summary>
        /// The list of bids
        /// </summary>
        public IEnumerable<ISymbolOrderBookEntry> Bids
        {
            get
            {
                lock (bookLock)
                    return bids.Select(a => a.Value).ToList();
            }
        }

        /// <summary>
        /// The best bid currently in the order book
        /// </summary>
        public ISymbolOrderBookEntry BestBid
        {
            get
            {
                lock (bookLock)
                    return bids.FirstOrDefault().Value;
            }
        }

        /// <summary>
        /// The best ask currently in the order book
        /// </summary>
        public ISymbolOrderBookEntry BestAsk
        {
            get
            {
                lock (bookLock)
                    return asks.FirstOrDefault().Value;
            }
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="options"></param>
        protected SymbolOrderBook(string symbol, OrderBookOptions options)
        {
            id = options.OrderBookName;
            processBuffer = new List<ProcessBufferEntry>();
            sequencesAreConsecutive = options.SequenceNumbersAreConsecutive;
            Symbol = symbol;
            Status = OrderBookStatus.Disconnected;

            asks = new SortedList<decimal, OrderBookEntry>();
            bids = new SortedList<decimal, OrderBookEntry>(new DescComparer<decimal>());

            log = new Log { Level = options.LogVerbosity };
            var writers = options.LogWriters ?? new List<TextWriter> { new DebugTextWriter() };
            log.UpdateWriters(writers.ToList());
        }

        /// <summary>
        /// Start connecting and synchronizing the order book
        /// </summary>
        /// <returns></returns>
        public CallResult<bool> Start() => StartAsync().Result;

        /// <summary>
        /// Start connecting and synchronizing the order book
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<bool>> StartAsync()
        {
            Status = OrderBookStatus.Connecting;
            var startResult = await DoStart().ConfigureAwait(false);
            if (!startResult.Success)
                return new CallResult<bool>(false, startResult.Error);

            subscription = startResult.Data;
            subscription.ConnectionLost += Reset;
            subscription.ConnectionRestored += (time) => Resync();
            Status = OrderBookStatus.Synced;
            return new CallResult<bool>(true, null);
        }

        private void Reset()
        {
            log.Write(LogVerbosity.Warning, $"{id} order book {Symbol} connection lost");
            Status = OrderBookStatus.Connecting;
            processBuffer.Clear();
            bookSet = false;
            DoReset();
        }

        private void Resync()
        {
            Status = OrderBookStatus.Syncing;
            bool success = false;
            while (!success)
            {
                if (Status != OrderBookStatus.Syncing)
                    return;

                var resyncResult = DoResync().Result;
                success = resyncResult.Success;
            }

            log.Write(LogVerbosity.Info, $"{id} order book {Symbol} successfully resynchronized");
            Status = OrderBookStatus.Synced;
        }

        /// <summary>
        /// Stop syncing the order book
        /// </summary>
        /// <returns></returns>
        public void Stop() => StopAsync().Wait();

        /// <summary>
        /// Stop syncing the order book
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            Status = OrderBookStatus.Disconnected;
            await subscription.Close().ConfigureAwait(false);
        }

        /// <summary>
        /// Start the order book
        /// </summary>
        /// <returns></returns>
        protected abstract Task<CallResult<UpdateSubscription>> DoStart();

        /// <summary>
        /// Reset the order book
        /// </summary>
        protected virtual void DoReset() { }

        /// <summary>
        /// Resync the order book
        /// </summary>
        /// <returns></returns>
        protected abstract Task<CallResult<bool>> DoResync();

        /// <summary>
        /// Set the initial data for the order book
        /// </summary>
        /// <param name="orderBookSequenceNumber">The last update sequence number</param>
        /// <param name="askList">List of asks</param>
        /// <param name="bidList">List of bids</param>
        protected void SetInitialOrderBook(long orderBookSequenceNumber, IEnumerable<ISymbolOrderBookEntry> askList, IEnumerable<ISymbolOrderBookEntry> bidList)
        {
            lock (bookLock)
            {
                if (Status == OrderBookStatus.Connecting)
                    return;

                asks.Clear();
                foreach (var ask in askList)
                    asks.Add(ask.Price, new OrderBookEntry(ask.Price, ask.Quantity));
                bids.Clear();
                foreach (var bid in bidList)
                    bids.Add(bid.Price, new OrderBookEntry(bid.Price, bid.Quantity));

                LastSequenceNumber = orderBookSequenceNumber;

                AskCount = asks.Count;
                BidCount = asks.Count;

                CheckProcessBuffer();
                bookSet = true;
                LastOrderBookUpdate = DateTime.UtcNow;
                OnOrderBookUpdate?.Invoke();
                log.Write(LogVerbosity.Debug, $"{id} order book {Symbol} data set: {BidCount} bids, {AskCount} asks");
            }
        }

        /// <summary>
        /// Update the order book with entries
        /// </summary>
        /// <param name="firstSequenceNumber">First sequence number</param>
        /// <param name="lastSequenceNumber">Last sequence number</param>
        /// <param name="entries">List of entries</param>
        protected void UpdateOrderBook(long firstSequenceNumber, long lastSequenceNumber, List<ProcessEntry> entries)
        {
            lock (bookLock)
            {
                if (lastSequenceNumber < LastSequenceNumber)
                    return;

                if (!bookSet)
                {
                    var entry = new ProcessBufferEntry()
                    {
                        FirstSequence = firstSequenceNumber,
                        LastSequence = lastSequenceNumber,
                        Entries = entries
                    };
                    processBuffer.Add(entry);
                    log.Write(LogVerbosity.Debug, $"{id} order book {Symbol} update before synced; buffering");
                }
                else if (sequencesAreConsecutive && firstSequenceNumber > LastSequenceNumber + 1)
                {
                    // Out of sync
                    log.Write(LogVerbosity.Warning, $"{id} order book {Symbol} out of sync, reconnecting");
                    subscription.Reconnect().Wait();
                }
                else
                {
                    foreach (var entry in entries)
                        ProcessUpdate(entry.Type, entry.Entry);
                    LastSequenceNumber = lastSequenceNumber;
                    CheckProcessBuffer();
                    LastOrderBookUpdate = DateTime.UtcNow;
                    OnOrderBookUpdate?.Invoke();
                    log.Write(LogVerbosity.Debug, $"{id} order book {Symbol} update: {entries.Count} entries processed");
                }
            }
        }

        /// <summary>
        /// Check and empty the process buffer; see what entries to update the book with
        /// </summary>
        protected void CheckProcessBuffer()
        {
            foreach (var bufferEntry in processBuffer.OrderBy(b => b.FirstSequence).ToList())
            {
                if (bufferEntry.LastSequence < LastSequenceNumber)
                {
                    processBuffer.Remove(bufferEntry);
                    continue;
                }

                if (bufferEntry.FirstSequence > LastSequenceNumber + 1)
                    break;

                foreach (var entry in bufferEntry.Entries)
                    ProcessUpdate(entry.Type, entry.Entry);
                processBuffer.Remove(bufferEntry);
                LastSequenceNumber = bufferEntry.LastSequence;
            }
        }

        /// <summary>
        /// Update order book with an entry
        /// </summary>
        /// <param name="type">Type of entry</param>
        /// <param name="entry">The entry</param>
        protected virtual void ProcessUpdate(OrderBookEntryType type, ISymbolOrderBookEntry entry)
        {
            var listToChange = type == OrderBookEntryType.Ask ? asks : bids;
            if (entry.Quantity == 0)
            {
                var bookEntry = listToChange.SingleOrDefault(i => i.Key == entry.Price);
                if (!bookEntry.Equals(default(KeyValuePair<decimal, OrderBookEntry>)))
                {
                    listToChange.Remove(entry.Price);
                    if (type == OrderBookEntryType.Ask) AskCount--;
                    else BidCount--;
                }
            }
            else
            {
                var bookEntry = listToChange.SingleOrDefault(i => i.Key == entry.Price);
                if (bookEntry.Equals(default(KeyValuePair<decimal, OrderBookEntry>)))
                {
                    listToChange.Add(entry.Price, new OrderBookEntry(entry.Price, entry.Quantity));
                    if (type == OrderBookEntryType.Ask) AskCount++;
                    else BidCount++;
                }
                else
                    bookEntry.Value.Quantity = entry.Quantity;
            }
        }

        /// <summary>
        /// Dispose the order book
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// String representation of the top 3 entries
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(3);
        }

        /// <summary>
        /// String representation of the top x entries
        /// </summary>
        /// <returns></returns>
        public string ToString(int numberOfEntries)
        {
            var result = "";
            result += $"Asks ({AskCount}): {Environment.NewLine}";
            foreach (var entry in Asks.Take(numberOfEntries).Reverse())
                result += $"  {entry.Price.ToString(CultureInfo.InvariantCulture).PadLeft(8)} | {entry.Quantity.ToString(CultureInfo.InvariantCulture).PadRight(8)}{Environment.NewLine}";

            result += $"Bids ({BidCount}): {Environment.NewLine}";
            foreach (var entry in Bids.Take(numberOfEntries))
                result += $"  {entry.Price.ToString(CultureInfo.InvariantCulture).PadLeft(8)} | {entry.Quantity.ToString(CultureInfo.InvariantCulture).PadRight(8)}{Environment.NewLine}";
            return result;
        }
    }

    internal class DescComparer<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            return Comparer<T>.Default.Compare(y, x);
        }
    }
}
