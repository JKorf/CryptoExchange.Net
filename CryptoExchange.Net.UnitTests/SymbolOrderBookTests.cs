using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.OrderBook;
using NUnit.Framework;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    public class SymbolOrderBookTests
    {
        private static readonly OrderBookOptions _defaultOrderBookOptions = new OrderBookOptions();

        private class TestableSymbolOrderBook : SymbolOrderBook
        {
            public TestableSymbolOrderBook() : base(null, "Test", "BTC/USD")
            {
                Initialize(_defaultOrderBookOptions);
            }


            protected override Task<CallResult<bool>> DoResyncAsync(CancellationToken ct)
            {
                throw new NotImplementedException();
            }

            protected override Task<CallResult<UpdateSubscription>> DoStartAsync(CancellationToken ct)
            {
                throw new NotImplementedException();
            }

            public void SetData(IEnumerable<ISymbolOrderBookEntry> bids, IEnumerable<ISymbolOrderBookEntry> asks)
            {
                Status = OrderBookStatus.Synced;
                base._bids.Clear();
                foreach (var bid in bids)
                    base._bids.Add(bid.Price, bid);
                base._asks.Clear();
                foreach (var ask in asks)
                    base._asks.Add(ask.Price, ask);
            }
        }

        public class BookEntry : ISymbolOrderBookEntry
        {
            public decimal Quantity { get; set; }
            public decimal Price { get; set; }
        }

        [TestCase]
        public void GivenEmptyBidList_WhenBestBid_ThenEmptySymbolOrderBookEntry()
        {
            var symbolOrderBook = new TestableSymbolOrderBook();
            Assert.IsNotNull(symbolOrderBook.BestBid);
            Assert.AreEqual(0m, symbolOrderBook.BestBid.Price);
            Assert.AreEqual(0m, symbolOrderBook.BestAsk.Quantity);
        }

        [TestCase]
        public void GivenEmptyAskList_WhenBestAsk_ThenEmptySymbolOrderBookEntry()
        {
            var symbolOrderBook = new TestableSymbolOrderBook();
            Assert.IsNotNull(symbolOrderBook.BestBid);
            Assert.AreEqual(0m, symbolOrderBook.BestBid.Price);
            Assert.AreEqual(0m, symbolOrderBook.BestAsk.Quantity);
        }

        [TestCase]
        public void GivenEmptyBidAndAskList_WhenBestOffers_ThenEmptySymbolOrderBookEntries()
        {
            var symbolOrderBook = new TestableSymbolOrderBook();
            Assert.IsNotNull(symbolOrderBook.BestOffers);
            Assert.IsNotNull(symbolOrderBook.BestOffers.Bid);
            Assert.IsNotNull(symbolOrderBook.BestOffers.Ask);
            Assert.AreEqual(0m, symbolOrderBook.BestOffers.Bid.Price);
            Assert.AreEqual(0m, symbolOrderBook.BestOffers.Bid.Quantity);
            Assert.AreEqual(0m, symbolOrderBook.BestOffers.Ask.Price);
            Assert.AreEqual(0m, symbolOrderBook.BestOffers.Ask.Quantity);
        }

        [TestCase]
        public void CalculateAverageFillPrice()
        {
            var orderbook = new TestableSymbolOrderBook();
            orderbook.SetData(new List<ISymbolOrderBookEntry>
            {
                new BookEntry{ Price = 1, Quantity = 1 },
                new BookEntry{ Price = 1.1m, Quantity = 1 },
            },
            new List<ISymbolOrderBookEntry>()
            {
                new BookEntry{ Price = 1.2m, Quantity = 1 },
                new BookEntry{ Price = 1.3m, Quantity = 1 },
            });

            var resultBids = orderbook.CalculateAverageFillPrice(2, OrderBookEntryType.Bid);
            var resultAsks = orderbook.CalculateAverageFillPrice(2, OrderBookEntryType.Ask);
            var resultBids2 = orderbook.CalculateAverageFillPrice(1.5m, OrderBookEntryType.Bid);
            var resultAsks2 = orderbook.CalculateAverageFillPrice(1.5m, OrderBookEntryType.Ask);

            Assert.True(resultBids.Success);
            Assert.True(resultAsks.Success);
            Assert.AreEqual(1.05m, resultBids.Data);
            Assert.AreEqual(1.25m, resultAsks.Data);
            Assert.AreEqual(1.06666667m, resultBids2.Data);
            Assert.AreEqual(1.23333333m, resultAsks2.Data);
        }

        [TestCase]
        public void CalculateTradableAmount()
        {
            var orderbook = new TestableSymbolOrderBook();
            orderbook.SetData(new List<ISymbolOrderBookEntry>
            {
                new BookEntry{ Price = 1, Quantity = 1 },
                new BookEntry{ Price = 1.1m, Quantity = 1 },
            },
            new List<ISymbolOrderBookEntry>()
            {
                new BookEntry{ Price = 1.2m, Quantity = 1 },
                new BookEntry{ Price = 1.3m, Quantity = 1 },
            });

            var resultBids = orderbook.CalculateTradableAmount(2, OrderBookEntryType.Bid);
            var resultAsks = orderbook.CalculateTradableAmount(2, OrderBookEntryType.Ask);
            var resultBids2 = orderbook.CalculateTradableAmount(1.5m, OrderBookEntryType.Bid);
            var resultAsks2 = orderbook.CalculateTradableAmount(1.5m, OrderBookEntryType.Ask);

            Assert.True(resultBids.Success);
            Assert.True(resultAsks.Success);
            Assert.AreEqual(1.9m, resultBids.Data);
            Assert.AreEqual(1.61538462m, resultAsks.Data);
            Assert.AreEqual(1.4m, resultBids2.Data);
            Assert.AreEqual(1.23076923m, resultAsks2.Data);
        }
    }
}
