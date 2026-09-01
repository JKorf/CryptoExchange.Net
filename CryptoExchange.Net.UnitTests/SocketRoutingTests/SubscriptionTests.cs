using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using CryptoExchange.Net.UnitTests.Implementations;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System;

namespace CryptoExchange.Net.UnitTests.SocketRoutingTests
{
    [TestFixture]
    public class SubscriptionTests
    {
        [Test]
        public void Handle_Should_OnlyCompleteSubscriptionQuery_ForMatchingTopic()
        {
            // arrange
            var topicASubscription = new TopicSubscription("topic-a");
            var topicBSubscription = new TopicSubscription("topic-b");
            var topicAQuery = topicASubscription.CreateSubscriptionQuery(null!)!;
            var topicBQuery = topicBSubscription.CreateSubscriptionQuery(null!)!;

            // act
            var topicAHandled = topicASubscription.Handle("type", "topic-a", null!, DateTime.UtcNow, "original", "data");
            var topicBHandled = topicBSubscription.Handle("type", "topic-a", null!, DateTime.UtcNow, "original", "data");

            // assert
            Assert.That(topicAHandled, Is.True);
            Assert.That(topicBHandled, Is.False);
            Assert.That(topicAQuery.Completed, Is.True);
            Assert.That(topicAQuery.Success, Is.True);
            Assert.That(topicBQuery.Completed, Is.False);
        }

        [Test]
        public void Handle_Should_CompleteSubscriptionQuery_BeforeInvokingMatchingHandler()
        {
            // arrange
            Query? query = null;
            var queryCompletedWhenHandlerInvoked = false;
            var subscription = new TopicSubscription("topic", () => queryCompletedWhenHandlerInvoked = query!.Completed);
            query = subscription.CreateSubscriptionQuery(null!)!;

            // act
            subscription.Handle("type", "topic", null!, DateTime.UtcNow, "original", "data");

            // assert
            Assert.That(queryCompletedWhenHandlerInvoked, Is.True);
        }

        [TestCase(null)]
        [TestCase("topic")]
        public void Handle_Should_CompleteSubscriptionQuery_ForUnfilteredRoute(string? topicFilter)
        {
            // arrange
            var subscription = new TopicSubscription(topic: null);
            var query = subscription.CreateSubscriptionQuery(null!)!;

            // act
            var handled = subscription.Handle("type", topicFilter, null!, DateTime.UtcNow, "original", "data");

            // assert
            Assert.That(handled, Is.True);
            Assert.That(query.Completed, Is.True);
            Assert.That(query.Success, Is.True);
        }

        [Test]
        public void Handle_Should_TreatEmptyTopicFilterAsUnfilteredRoute()
        {
            // arrange
            var subscription = new TopicSubscription(string.Empty);
            var query = subscription.CreateSubscriptionQuery(null!)!;

            // act
            var handled = subscription.Handle("type", "topic", null!, DateTime.UtcNow, "original", "data");

            // assert
            Assert.That(handled, Is.True);
            Assert.That(query.Completed, Is.True);
            Assert.That(query.Success, Is.True);
        }

        [Test]
        public void Handle_Should_CompleteSubscriptionQuery_ForAnyMatchingTopic()
        {
            // arrange
            var subscription = new TopicSubscription(["topic-a", "topic-b"]);
            var query = subscription.CreateSubscriptionQuery(null!)!;

            // act
            var handled = subscription.Handle("type", "topic-b", null!, DateTime.UtcNow, "original", "data");

            // assert
            Assert.That(handled, Is.True);
            Assert.That(query.Completed, Is.True);
            Assert.That(query.Success, Is.True);
        }

        private sealed class TopicSubscription : Subscription
        {
            public TopicSubscription(string? topic, Action? handler = null)
                : base(NullLogger.Instance, false)
            {
                MessageRouter = MessageRouter.CreateForEvent<string>("type", topic, (_, _, _, _) =>
                {
                    handler?.Invoke();
                    return CallResult.Ok();
                });
            }

            public TopicSubscription(string[] topics)
                : base(NullLogger.Instance, false)
            {
                MessageRouter = MessageRouter.CreateForEvent<string>("type", topics, (_, _, _, _) => CallResult.Ok());
            }

            protected override Query? GetSubQuery(SocketConnection connection)
            {
                return new TestQuery(new TestSocketMessage { Id = 1, Data = "Sub" }, false)
                {
                    TimeoutBehavior = TimeoutBehavior.Succeed
                };
            }

            protected override Query? GetUnsubQuery(SocketConnection connection) => null;
        }
    }
}
