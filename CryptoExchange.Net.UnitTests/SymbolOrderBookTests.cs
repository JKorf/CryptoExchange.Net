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
using NUnit.Framework.Legacy;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture]
    public class SymbolOrderBookTests
    {
        private static readonly OrderBookOptions _defaultOrderBookOptions = new OrderBookOptions();

        private class TestableSymbolOrderBook : SymbolOrderBook
        {
            public TestableSymbolOrderBook() : base(null, "Test", "Test", "BTC/USD")
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
            ClassicAssert.IsNotNull(symbolOrderBook.BestBid);
            Assert.That(0m == symbolOrderBook.BestBid.Price);
            Assert.That(0m == symbolOrderBook.BestAsk.Quantity);
        }

        [TestCase]
        public void GivenEmptyAskList_WhenBestAsk_ThenEmptySymbolOrderBookEntry()
        {
            var symbolOrderBook = new TestableSymbolOrderBook();
            ClassicAssert.IsNotNull(symbolOrderBook.BestBid);
            Assert.That(0m == symbolOrderBook.BestBid.Price);
            Assert.That(0m == symbolOrderBook.BestAsk.Quantity);
        }

        [TestCase]
        public void GivenEmptyBidAndAskList_WhenBestOffers_ThenEmptySymbolOrderBookEntries()
        {
            var symbolOrderBook = new TestableSymbolOrderBook();
            ClassicAssert.IsNotNull(symbolOrderBook.BestOffers);
            ClassicAssert.IsNotNull(symbolOrderBook.BestOffers.Bid);
            ClassicAssert.IsNotNull(symbolOrderBook.BestOffers.Ask);
            Assert.That(0m == symbolOrderBook.BestOffers.Bid.Price);
            Assert.That(0m == symbolOrderBook.BestOffers.Bid.Quantity);
            Assert.That(0m == symbolOrderBook.BestOffers.Ask.Price);
            Assert.That(0m == symbolOrderBook.BestOffers.Ask.Quantity);
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

            Assert.That(resultBids.Success);
            Assert.That(resultAsks.Success);
            Assert.That(1.05m == resultBids.Data);
            Assert.That(1.25m == resultAsks.Data);
            Assert.That(1.06666667m == resultBids2.Data);
            Assert.That(1.23333333m == resultAsks2.Data);
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

            Assert.That(resultBids.Success);
            Assert.That(resultAsks.Success);
            Assert.That(1.9m == resultBids.Data);
            Assert.That(1.61538462m == resultAsks.Data);
            Assert.That(1.4m == resultBids2.Data);
            Assert.That(1.23076923m == resultAsks2.Data);
        }
    }
}
