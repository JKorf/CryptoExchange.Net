using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
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
        protected readonly List<ProcessBufferRangeSequenceEntry> processBuffer;
        /// <summary>
        /// The ask list
        /// </summary>
        protected SortedList<decimal, ISymbolOrderBookEntry> asks;
        /// <summary>
        /// The bid list
        /// </summary>
        protected SortedList<decimal, ISymbolOrderBookEntry> bids;

        private readonly object bookLock = new object();

        private OrderBookStatus status;
        private UpdateSubscription? subscription;
        private readonly bool sequencesAreConsecutive;
        private readonly bool strictLevels;

        private Task? _processTask;
        private readonly AutoResetEvent _queueEvent;
        private readonly ConcurrentQueue<object> _processQueue;

        /// <summary>
        /// Order book implementation id
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// The log
        /// </summary>
        protected Log log;

        /// <summary>
        /// If order book is set
        /// </summary>
        protected bool bookSet;

        /// <summary>
        /// The amount of levels for this book
        /// </summary>
        protected int? Levels { get; set; } = null;

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
        /// Event when the BestBid or BestAsk changes ie a Pricing Tick
        /// </summary>
        public event Action<(ISymbolOrderBookEntry BestBid, ISymbolOrderBookEntry BestAsk)>? OnBestOffersChanged;

        /// <summary>
        /// Event when order book was updated, containing the changed bids and asks. Be careful! It can generate a lot of events at high-liquidity markets 
        /// </summary>
        public event Action<(IEnumerable<ISymbolOrderBookEntry> Bids, IEnumerable<ISymbolOrderBookEntry> Asks)>? OnOrderBookUpdate;
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

        private class EmptySymbolOrderBookEntry : ISymbolOrderBookEntry
        {
            public decimal Quantity { get { return 0m; } set {; } }
            public decimal Price { get { return 0m; } set {; } }
        }

        private static readonly ISymbolOrderBookEntry emptySymbolOrderBookEntry = new EmptySymbolOrderBookEntry();

        /// <summary>
        /// The best bid currently in the order book
        /// </summary>
        public ISymbolOrderBookEntry BestBid
        {
            get
            {
                lock (bookLock)
                    return bids.FirstOrDefault().Value ?? emptySymbolOrderBookEntry;
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
                    return asks.FirstOrDefault().Value ?? emptySymbolOrderBookEntry;
            }
        }

        /// <summary>
        /// BestBid/BesAsk returned as a pair
        /// </summary>
        public (ISymbolOrderBookEntry Bid, ISymbolOrderBookEntry Ask) BestOffers {
            get {
                lock (bookLock)
                    return (BestBid,BestAsk);
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
            processBuffer = new List<ProcessBufferRangeSequenceEntry>();
            _processQueue = new ConcurrentQueue<object>();
            _queueEvent = new AutoResetEvent(false);

            sequencesAreConsecutive = options.SequenceNumbersAreConsecutive;
            strictLevels = options.StrictLevels;
            Symbol = symbol;
            Status = OrderBookStatus.Disconnected;

            asks = new SortedList<decimal, ISymbolOrderBookEntry>();
            bids = new SortedList<decimal, ISymbolOrderBookEntry>(new DescComparer<decimal>());

            log = new Log(options.OrderBookName) { Level = options.LogVerbosity };
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
            log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} starting");
            Status = OrderBookStatus.Connecting;
            _processTask = Task.Run(ProcessQueue);

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
            Status = OrderBookStatus.Reconnecting;
            _queueEvent.Set();
            // Clear queue
            while(_processQueue.TryDequeue(out _))
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
            log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} stopping");
            Status = OrderBookStatus.Disconnected;
            _queueEvent.Set();
            _processTask?.Wait();
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
        /// Validate a checksum with the current order book
        /// </summary>
        /// <param name="checksum"></param>
        /// <returns></returns>
        protected virtual bool DoChecksum(int checksum) => true;

        private void ProcessQueue()
        {
            while(Status != OrderBookStatus.Disconnected)
            {
                _queueEvent.WaitOne();

                while (_processQueue.TryDequeue(out var item))
                {
                    if (Status == OrderBookStatus.Disconnected)
                        break;

                    if (item is InitialOrderBookItem iobi)
                        ProcessInitialOrderBookItem(iobi);
                    if (item is ProcessQueueItem pqi)
                        ProcessQueueItem(pqi);
                    else if (item is ChecksumItem ci)
                        ProcessChecksum(ci);
                }
            }
        }

        private void ProcessInitialOrderBookItem(InitialOrderBookItem item)
        {
            lock (bookLock)
            {
                if (Status == OrderBookStatus.Connecting || Status == OrderBookStatus.Disconnected)
                    return;

                asks.Clear();
                foreach (var ask in item.Asks)
                    asks.Add(ask.Price, ask);
                bids.Clear();
                foreach (var bid in item.Bids)
                    bids.Add(bid.Price, bid);

                LastSequenceNumber = item.EndUpdateId;

                AskCount = asks.Count;
                BidCount = bids.Count;

                LastOrderBookUpdate = DateTime.UtcNow;
                log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} data set: {BidCount} bids, {AskCount} asks. #{item.EndUpdateId}");
                CheckProcessBuffer();
                OnOrderBookUpdate?.Invoke((item.Bids, item.Asks));
                OnBestOffersChanged?.Invoke((BestBid, BestAsk));
            }
        }

        private void ProcessQueueItem(ProcessQueueItem item)
        {
            lock (bookLock)
            {
                if (Status == OrderBookStatus.Connecting || Status == OrderBookStatus.Disconnected)
                    return;

                if (!bookSet)
                {
                    processBuffer.Add(new ProcessBufferRangeSequenceEntry()
                    {
                        Asks = item.Asks,
                        Bids = item.Bids,
                        FirstUpdateId = item.StartUpdateId,
                        LastUpdateId = item.EndUpdateId,
                    });
                    log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} update buffered #{item.StartUpdateId}-#{item.EndUpdateId} [{item.Asks.Count()} asks, {item.Bids.Count()} bids]");
                }
                else
                {
                    CheckProcessBuffer();
                    var (prevBestBid, prevBestAsk) = BestOffers;
                    ProcessRangeUpdates(item.StartUpdateId, item.EndUpdateId, item.Bids, item.Asks);

                    if (asks.First().Key < bids.First().Key)
                    {
                        log.Write(LogVerbosity.Warning, $"{Id} order book {Symbol} detected out of sync order book. Resyncing");
                        _ = subscription?.Reconnect();
                        return;
                    }

                    OnOrderBookUpdate?.Invoke((item.Bids, item.Asks));
                    CheckBestOffersChanged(prevBestBid, prevBestAsk);
                }
            }
        }

        private void ProcessChecksum(ChecksumItem ci)
        {
            lock (bookLock)
            {
                bool checksumResult = false;
                try
                {
                    checksumResult = DoChecksum(ci.Checksum);
                }
                catch (Exception)
                {
                    // If the status is not synced it can be expected a checksum is failing

                    if (Status == OrderBookStatus.Synced)
                        throw;
                }

                if(!checksumResult)
                {
                    log.Write(LogVerbosity.Warning, $"{Id} order book {Symbol} out of sync. Resyncing");
                    _ = subscription?.Reconnect();
                    return;
                }
            }
        }

        /// <summary>
        /// Set the initial data for the order book
        /// </summary>
        /// <param name="orderBookSequenceNumber">The last update sequence number</param>
        /// <param name="askList">List of asks</param>
        /// <param name="bidList">List of bids</param>
        protected void SetInitialOrderBook(long orderBookSequenceNumber, IEnumerable<ISymbolOrderBookEntry> bidList, IEnumerable<ISymbolOrderBookEntry> askList)
        {
            bookSet = true;

            _processQueue.Enqueue(new InitialOrderBookItem { StartUpdateId = orderBookSequenceNumber, EndUpdateId = orderBookSequenceNumber, Asks = askList, Bids = bidList });
            _queueEvent.Set();
        }

        /// <summary>
        /// Update the order book using a single id for an update
        /// </summary>
        /// <param name="rangeUpdateId"></param>
        /// <param name="bids"></param>
        /// <param name="asks"></param>
        protected void UpdateOrderBook(long rangeUpdateId, IEnumerable<ISymbolOrderBookEntry> bids, IEnumerable<ISymbolOrderBookEntry> asks)
        {
            _processQueue.Enqueue(new ProcessQueueItem { StartUpdateId = rangeUpdateId, EndUpdateId = rangeUpdateId, Asks = asks, Bids = bids });
            _queueEvent.Set();
        }

        /// <summary>
        /// Add a checksum to the process queue
        /// </summary>
        /// <param name="checksum"></param>
        protected void AddChecksum(int checksum)
        {
            _processQueue.Enqueue(new ChecksumItem() { Checksum = checksum });
            _queueEvent.Set();
        }

        /// <summary>
        /// Update the order book using a first/last update id
        /// </summary>
        /// <param name="firstUpdateId"></param>
        /// <param name="lastUpdateId"></param>
        /// <param name="bids"></param>
        /// <param name="asks"></param>
        protected void UpdateOrderBook(long firstUpdateId, long lastUpdateId, IEnumerable<ISymbolOrderBookEntry> bids, IEnumerable<ISymbolOrderBookEntry> asks)
        {
            _processQueue.Enqueue(new ProcessQueueItem { StartUpdateId = firstUpdateId, EndUpdateId = lastUpdateId, Asks = asks, Bids = bids });
            _queueEvent.Set();
        }

        /// <summary>
        /// Update the order book using sequenced entries
        /// </summary>
        /// <param name="bids">List of bids</param>
        /// <param name="asks">List of asks</param>
        protected void UpdateOrderBook(IEnumerable<ISymbolOrderSequencedBookEntry> bids, IEnumerable<ISymbolOrderSequencedBookEntry> asks)
        {
            var highest = Math.Max(bids.Any() ? bids.Max(b => b.Sequence) : 0, asks.Any() ? asks.Max(a => a.Sequence) : 0);
            var lowest = Math.Min(bids.Any() ? bids.Min(b => b.Sequence) : long.MaxValue, asks.Any() ? asks.Min(a => a.Sequence) : long.MaxValue);

            _processQueue.Enqueue(new ProcessQueueItem { StartUpdateId = lowest, EndUpdateId = highest , Asks = asks, Bids = bids });
            _queueEvent.Set();
        }

        private void ProcessRangeUpdates(long firstUpdateId, long lastUpdateId, IEnumerable<ISymbolOrderBookEntry> bids, IEnumerable<ISymbolOrderBookEntry> asks)
        {
            if (lastUpdateId < LastSequenceNumber)
            {
                log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} update skipped #{firstUpdateId}-{lastUpdateId}");
                return;
            }

            foreach (var entry in bids)
                ProcessUpdate(LastSequenceNumber + 1, OrderBookEntryType.Bid, entry);

            foreach (var entry in asks)
                ProcessUpdate(LastSequenceNumber + 1, OrderBookEntryType.Ask, entry);

            if (Levels.HasValue && strictLevels)
            {
                while (this.bids.Count() > Levels.Value)
                {
                    BidCount--;
                    this.bids.Remove(this.bids.Last().Key);
                }

                while (this.asks.Count() > Levels.Value)
                {
                    AskCount--;
                    this.asks.Remove(this.asks.Last().Key);
                }
            }

            LastSequenceNumber = lastUpdateId;
            log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} update processed #{firstUpdateId}-{lastUpdateId}");
        }

        /// <summary>
        /// Check and empty the process buffer; see what entries to update the book with
        /// </summary>
        protected void CheckProcessBuffer()
        {
            var pbList = processBuffer.ToList();
            if(pbList.Count > 0)
                log.Write(LogVerbosity.Debug, "Processing buffered updates");

            foreach (var bufferEntry in pbList)
            {
                ProcessRangeUpdates(bufferEntry.FirstUpdateId, bufferEntry.LastUpdateId, bufferEntry.Bids, bufferEntry.Asks);
                processBuffer.Remove(bufferEntry);
            }
        }

        /// <summary>
        /// Update order book with an entry
        /// </summary>
        /// <param name="sequence">Sequence number of the update</param>
        /// <param name="type">Type of entry</param>
        /// <param name="entry">The entry</param>
        protected virtual bool ProcessUpdate(long sequence, OrderBookEntryType type, ISymbolOrderBookEntry entry)
        {
            if (Status != OrderBookStatus.Syncing && Status != OrderBookStatus.Synced)
                return false;

            if (sequence <= LastSequenceNumber)
            {
                log.Write(LogVerbosity.Debug, $"{Id} order book {Symbol} update skipped #{sequence}");
                return false;
            }

            if (sequencesAreConsecutive && sequence > LastSequenceNumber + 1)
            {
                // Out of sync
                log.Write(LogVerbosity.Warning, $"{Id} order book {Symbol} out of sync (expected { LastSequenceNumber + 1}, was {sequence}), reconnecting");
                subscription?.Reconnect();
                return false;
            }

            LastOrderBookUpdate = DateTime.UtcNow;
            var listToChange = type == OrderBookEntryType.Ask ? asks : bids;
            if (entry.Quantity == 0)
            {
                if (!listToChange.ContainsKey(entry.Price))
                    return true;

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
                    listToChange[entry.Price] = entry;
                }
            }

            return true;
        }

        /// <summary>
        /// Wait until the order book has been set
        /// </summary>
        /// <param name="timeout">Max wait time</param>
        /// <returns></returns>
        protected async Task<CallResult<bool>> WaitForSetOrderBook(int timeout)
        {
            var startWait = DateTime.UtcNow;
            while (!bookSet && Status == OrderBookStatus.Syncing)
            {
                if ((DateTime.UtcNow - startWait).TotalMilliseconds > timeout)
                    return new CallResult<bool>(false, new ServerError("Timeout while waiting for data"));

                await Task.Delay(10).ConfigureAwait(false);
            }

            return new CallResult<bool>(true, null);
        }

        private void CheckBestOffersChanged(ISymbolOrderBookEntry prevBestBid, ISymbolOrderBookEntry prevBestAsk)
        {
            var (bestBid, bestAsk) = BestOffers;
            if (bestBid.Price != prevBestBid.Price || bestBid.Quantity != prevBestBid.Quantity ||
                   bestAsk.Price != prevBestAsk.Price || bestAsk.Quantity != prevBestAsk.Quantity)
                OnBestOffersChanged?.Invoke((bestBid, bestAsk));
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
