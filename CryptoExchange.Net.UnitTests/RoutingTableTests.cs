using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using CryptoExchange.Net.Sockets.Interfaces;
using NUnit.Framework;
using System;

namespace CryptoExchange.Net.UnitTests
{
    internal class RoutingTableTests
    {
        [Test]
        public void Constructor_CreatesEntriesAndDeduplicatesHandlersPerTypeIdentifier()
        {
            var processor = new TestMessageProcessor(1, MessageRouter.Create(
                MessageRoute<string>.CreateWithoutTopicFilter("ticker", EmptyHandler<string>()),
                MessageRoute<string>.CreateWithTopicFilter("ticker", "btcusdt", EmptyHandler<string>()),
                MessageRoute<int>.CreateWithoutTopicFilter("trade", EmptyHandler<int>())));

            var routingTable = new RoutingTable(new[] { processor });
            var tickerEntry = routingTable.GetRouteTableEntry("ticker");
            var tradeEntry = routingTable.GetRouteTableEntry("trade");

            Assert.Multiple(() =>
            {
                Assert.That(tickerEntry, Is.Not.Null);
                Assert.That(tickerEntry!.DeserializationType, Is.EqualTo(typeof(string)));
                Assert.That(tickerEntry.IsStringOutput, Is.True);
                Assert.That(tickerEntry.Handlers, Has.Count.EqualTo(1));
                Assert.That(tickerEntry.Handlers[0], Is.SameAs(processor));

                Assert.That(tradeEntry, Is.Not.Null);
                Assert.That(tradeEntry!.DeserializationType, Is.EqualTo(typeof(int)));
                Assert.That(tradeEntry.IsStringOutput, Is.False);
                Assert.That(tradeEntry.Handlers, Has.Count.EqualTo(1));
                Assert.That(tradeEntry.Handlers[0], Is.SameAs(processor));
            });
        }

        [Test]
        public void Constructor_AddsAllProcessorsForSameTypeIdentifier()
        {
            var processor1 = new TestMessageProcessor(1, MessageRouter.Create(
                MessageRoute<string>.CreateWithoutTopicFilter("ticker", EmptyHandler<string>())));
            var processor2 = new TestMessageProcessor(2, MessageRouter.Create(
                MessageRoute<string>.CreateWithTopicFilter("ticker", "ethusdt", EmptyHandler<string>())));

            var routingTable = new RoutingTable(new IMessageProcessor[] { processor1, processor2 });
            var entry = routingTable.GetRouteTableEntry("ticker");

            Assert.Multiple(() =>
            {
                Assert.That(entry, Is.Not.Null);
                Assert.That(entry!.Handlers, Has.Count.EqualTo(2));
                Assert.That(entry.Handlers, Does.Contain(processor1));
                Assert.That(entry.Handlers, Does.Contain(processor2));
            });
        }

        [Test]
        public void GetRouteTableEntry_ReturnsNullForUnknownTypeIdentifier()
        {
            var processor = new TestMessageProcessor(1, MessageRouter.Create(
                MessageRoute<string>.CreateWithoutTopicFilter("ticker", EmptyHandler<string>())));

            var routingTable = new RoutingTable(new[] { processor });

            Assert.That(routingTable.GetRouteTableEntry("unknown"), Is.Null);
        }

        private static Func<SocketConnection, DateTime, string?, T, CallResult?> EmptyHandler<T>()
        {
            return (_, _, _, _) => null;
        }

        private class TestMessageProcessor : IMessageProcessor
        {
            public int Id { get; }
            public MessageRouter MessageRouter { get; }
            public event Action? OnMessageRouterUpdated;

            public TestMessageProcessor(int id, MessageRouter messageRouter)
            {
                Id = id;
                MessageRouter = messageRouter;
            }

            public bool Handle(string typeIdentifier, string? topicFilter, SocketConnection socketConnection, DateTime receiveTime, string? originalData, object result)
            {
                return false;
            }
        }
    }
}
