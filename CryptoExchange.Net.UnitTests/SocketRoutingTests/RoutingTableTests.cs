using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using CryptoExchange.Net.Sockets.Interfaces;
using NUnit.Framework;
using System;
using System.Linq;

namespace CryptoExchange.Net.UnitTests.SocketRoutingTests
{
    [TestFixture]
    public class RoutingTableTests
    {
        [Test]
        public void Update_Should_CreateEntriesPerTypeIdentifier_WithCorrectDeserializationTypeAndHandlers()
        {
            // arrange
            var processor1 = new TestMessageProcessor(
                1,
                MessageRouter.Create(
                    MessageRoute<string>.CreateWithoutTopicFilter("type1", (_, _, _, _) => null),
                    MessageRoute<string>.CreateWithTopicFilter("type1", "topic1", (_, _, _, _) => null)));

            var processor2 = new TestMessageProcessor(
                2,
                MessageRouter.Create(
                    MessageRoute<int>.CreateWithTopicFilter("type2", "topic2", (_, _, _, _) => null)));

            var table = new RoutingTable();

            // act
            table.Update(new IMessageProcessor[] { processor1, processor2 });

            var type1Entry = table.GetRouteTableEntry("type1");
            var type2Entry = table.GetRouteTableEntry("type2");
            var missingEntry = table.GetRouteTableEntry("missing");

            // assert
            Assert.That(type1Entry, Is.Not.Null);
            Assert.That(type2Entry, Is.Not.Null);
            Assert.That(missingEntry, Is.Null);

            Assert.That(type1Entry!.DeserializationType, Is.EqualTo(typeof(string)));
            Assert.That(type1Entry.IsStringOutput, Is.True);
            Assert.That(type1Entry.Handlers, Has.Count.EqualTo(1));
            Assert.That(type1Entry.Handlers.Single(), Is.SameAs(processor1));

            Assert.That(type2Entry!.DeserializationType, Is.EqualTo(typeof(int)));
            Assert.That(type2Entry.IsStringOutput, Is.False);
            Assert.That(type2Entry.Handlers, Has.Count.EqualTo(1));
            Assert.That(type2Entry.Handlers.Single(), Is.SameAs(processor2));
        }

        [Test]
        public void Update_Should_AddMultipleProcessors_ForSameTypeIdentifier()
        {
            // arrange
            var processor1 = new TestMessageProcessor(
                1,
                MessageRouter.Create(
                    MessageRoute<string>.CreateWithoutTopicFilter("type1", (_, _, _, _) => null)));

            var processor2 = new TestMessageProcessor(
                2,
                MessageRouter.Create(
                    MessageRoute<string>.CreateWithTopicFilter("type1", "topic1", (_, _, _, _) => null)));

            var table = new RoutingTable();

            // act
            table.Update(new IMessageProcessor[] { processor1, processor2 });
            var entry = table.GetRouteTableEntry("type1");

            // assert
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry!.DeserializationType, Is.EqualTo(typeof(string)));
            Assert.That(entry.Handlers, Has.Count.EqualTo(2));
            Assert.That(entry.Handlers, Does.Contain(processor1));
            Assert.That(entry.Handlers, Does.Contain(processor2));
        }

        [Test]
        public void Update_Should_ReplacePreviousEntries()
        {
            // arrange
            var initialProcessor = new TestMessageProcessor(
                1,
                MessageRouter.Create(
                    MessageRoute<string>.CreateWithoutTopicFilter("type1", (_, _, _, _) => null)));

            var replacementProcessor = new TestMessageProcessor(
                2,
                MessageRouter.Create(
                    MessageRoute<int>.CreateWithoutTopicFilter("type2", (_, _, _, _) => null)));

            var table = new RoutingTable();
            table.Update(new IMessageProcessor[] { initialProcessor });

            // act
            table.Update(new IMessageProcessor[] { replacementProcessor });

            var oldEntry = table.GetRouteTableEntry("type1");
            var newEntry = table.GetRouteTableEntry("type2");

            // assert
            Assert.That(oldEntry, Is.Null);
            Assert.That(newEntry, Is.Not.Null);
            Assert.That(newEntry!.DeserializationType, Is.EqualTo(typeof(int)));
            Assert.That(newEntry.Handlers, Has.Count.EqualTo(1));
            Assert.That(newEntry.Handlers.Single(), Is.SameAs(replacementProcessor));
        }

        [Test]
        public void Update_WithEmptyProcessors_Should_ClearEntries()
        {
            // arrange
            var processor = new TestMessageProcessor(
                1,
                MessageRouter.Create(
                    MessageRoute<string>.CreateWithoutTopicFilter("type1", (_, _, _, _) => null)));

            var table = new RoutingTable();
            table.Update(new IMessageProcessor[] { processor });

            // act
            table.Update(Array.Empty<IMessageProcessor>());

            // assert
            Assert.That(table.GetRouteTableEntry("type1"), Is.Null);
        }

        [Test]
        public void TypeRoutingCollection_Should_SetIsStringOutput_BasedOnDeserializationType()
        {
            // arrange & act
            var stringCollection = new TypeRoutingCollection(typeof(string));
            var intCollection = new TypeRoutingCollection(typeof(int));

            // assert
            Assert.That(stringCollection.IsStringOutput, Is.True);
            Assert.That(stringCollection.DeserializationType, Is.EqualTo(typeof(string)));
            Assert.That(stringCollection.Handlers, Is.Empty);

            Assert.That(intCollection.IsStringOutput, Is.False);
            Assert.That(intCollection.DeserializationType, Is.EqualTo(typeof(int)));
            Assert.That(intCollection.Handlers, Is.Empty);
        }

        private sealed class TestMessageProcessor : IMessageProcessor
        {
            public int Id { get; }
            public MessageRouter MessageRouter { get; }

            public TestMessageProcessor(int id, MessageRouter messageRouter)
            {
                Id = id;
                MessageRouter = messageRouter;
            }

            public event Action? OnMessageRouterUpdated;

            public bool Handle(string typeIdentifier, string? topicFilter, SocketConnection socketConnection, DateTime receiveTime, string? originalData, object result)
            {
                return true;
            }
        }
    }
}