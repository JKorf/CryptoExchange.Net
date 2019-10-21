using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

namespace CryptoExchange.Net.OrderBook
{
    /// <summary>
    /// Base for order book implementations
    /// </summary>
    public abstract class SymbolOrderBook : ISymbolOrderBook, IDisposable
    {
        /// <summary>
        /// The process buffer, used while syncing
        /// </summary>
        protected readonly List<ProcessBufferEntry> processBuffer;
        private readonly object bookLock = new object();
        /// <summary>
        /// The ask list
        /// </summary>
        protected SortedList<decimal, ISymbolOrderBookEntry> asks;
        /// <summary>
        /// The bid list
        /// </summary>

        protected SortedList<decimal, ISymbolOrderBookEntry> bids;
        private OrderBookStatus status;
        private UpdateSubscription? subscription;
        private readonly bool sequencesAreConsecutive;

        /// <summary>
        /// Order book implementation id
        /// </summary>
        public string Id { get; }
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
                log.Write(LogVerbosity.Info, $"{Id} order book {Symbol} status changed: {old} => {value}");
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
        public event Action<OrderBookStatus, OrderBookStatus>? OnStatusChange;
        /// <summary>
        /// Event when order book was updated, containing the changed bids and asks. Be careful! It can generate a lot of events at high-liquidity markets
        /// </summary>    
        public event Action<IEnumerable<ISymbolOrderBookEntry>, IEnumerable<ISymbolOrderBookEntry>>? OnOrderBookUpdate;
        /// <summary>
        /// Timestamp of the last update
        /// </summary>
        public DateTime LastOrderBookUpdate { get; private set; }

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
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Id = options.OrderBookName;
            processBuffer = new List<ProcessBufferEntry>();
            sequencesAreConsecutive = options.SequenceNumbersAreConsecutive;
            Symbol = symbol;
            Status = OrderBookStatus.Disconnected;

            asks = new SortedList<decimal, ISymbolOrderBookEntry>();
            bids = new SortedList<decimal, ISymbolOrderBookEntry>(new DescComparer<decimal>());

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
            if (!startResult)
                return new CallResult<bool>(false, startResult.Error);

            subscription = startResult.Data;
            subscription.ConnectionLost += Reset;
            subscription.ConnectionRestored += time => Resync();
            Status = OrderBookStatus.Synced;
            return new CallResult<bool>(true, null);
        }

        private void Reset()
        {
            log.Write(LogVerbosity.Warning, $"{Id} order book {Symbol} connection lost");
            Status = OrderBookStatus.Connecting;
            processBuffer.Clear();
            bookSet = false;
            DoReset();
        }

        private void Resync()
        {
            Status = OrderBookStatus.Syncing;
            var success = false;
            while (!success)
            {
                if (Status != OrderBookStatus.Syncing)
                    return;

                var resyncResult = DoResync().Result;
                success = resyncResult;
            }

            log.Write(LogVerbosity.Info, $"{Id} order book {Symbol} successfully resynchronized");
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
            if(subscription != null)
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
                    asks.Add(ask.Price, ask);
                bids.Clear();
                foreach (var bid in bidList)
                    bids.Add(bid.Price, bid);

                LastSequenceNumber = orderBookSequenceNumber;

                AskCount = asks.Count;
                BidCount = asks.Count;

                CheckProcessBuffer();
                bookSet = true;
                LastOrderBookUpdate = DateTime.UtcNow;
                OnOrderBookUpdate?.Invoke(bidList, askList);
                log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} data set: {BidCount} bids, {AskCount} asks");
            }
        }

        /// <summary>
        /// Update the order book with entries
        /// </summary>
        /// <param name="firstSequenceNumber">First sequence number</param>
        /// <param name="lastSequenceNumber">Last sequence number</param>
        /// <param name="bids">List of bids</param>
        /// <param name="asks">List of asks</param>
        protected void UpdateOrderBook(long firstSequenceNumber, long lastSequenceNumber, IEnumerable<ISymbolOrderBookEntry> bids, IEnumerable<ISymbolOrderBookEntry> asks)
        {
            lock (bookLock)
            {
                if (lastSequenceNumber < LastSequenceNumber)
                    return;

                if (!bookSet)
                {
                    var entry = new ProcessBufferEntry
                    {
                        FirstSequence = firstSequenceNumber,
                        LastSequence = lastSequenceNumber,
                        Asks = asks,
                        Bids = bids
                    };
                    processBuffer.Add(entry);
                    log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} update before synced; buffering");
                }
                else if (sequencesAreConsecutive && firstSequenceNumber > LastSequenceNumber + 1)
                {
                    // Out of sync
                    log.Write(LogVerbosity.Warning, $"{Id} order book {Symbol} out of sync, reconnecting");
                    subscription!.Reconnect().Wait();
                }
                else
                {
                    foreach (var entry in asks)
                        ProcessUpdate(OrderBookEntryType.Ask, entry);
                    foreach (var entry in bids)
                        ProcessUpdate(OrderBookEntryType.Bid, entry);
                    LastSequenceNumber = lastSequenceNumber;
                    CheckProcessBuffer();
                    LastOrderBookUpdate = DateTime.UtcNow;
                    OnOrderBookUpdate?.Invoke(bids, asks);
                    log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} update: {asks.Count()} asks, {bids.Count()} bids processed");
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

                foreach (var entry in bufferEntry.Asks)
                    ProcessUpdate(OrderBookEntryType.Ask, entry);
                foreach (var entry in bufferEntry.Bids)
                    ProcessUpdate(OrderBookEntryType.Bid, entry);

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
                if (!listToChange.ContainsKey(entry.Price))
                    return;

                listToChange.Remove(entry.Price);
                if (type == OrderBookEntryType.Ask) AskCount--;
                else BidCount--;
            }
            else
            {
                if (!listToChange.ContainsKey(entry.Price))
                {
                    listToChange.Add(entry.Price, entry);
                    if (type == OrderBookEntryType.Ask) AskCount++;
                    else BidCount++;
                }
                else
                {
                    listToChange[entry.Price].Quantity = entry.Quantity;
                }
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
