using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets.Default.Routing;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.UnitTests.SocketRoutingTests
{
    [TestFixture]
    public class SubscriptionRouterTests
    {
        [Test]
        public void BuildFromRoutes_Should_GroupRoutesByTypeIdentifier_AndSetDeserializationType()
        {
            // arrange
            var routes = new MessageRoute[]
            {
                MessageRoute<string>.CreateWithoutTopicFilter("type1", (_, _, _, _) => null),
                MessageRoute<string>.CreateWithTopicFilter("type1", "topic1", (_, _, _, _) => null),
                MessageRoute<int>.CreateWithTopicFilter("type2", "topic2", (_, _, _, _) => null)
            };

            var router = new SubscriptionRouter(routes);

            // act
            var type1Routes = router.GetRoutes("type1");
            var type2Routes = router.GetRoutes("type2");
            var missingRoutes = router.GetRoutes("missing");

            // assert
            Assert.That(type1Routes, Is.Not.Null);
            Assert.That(type2Routes, Is.Not.Null);
            Assert.That(missingRoutes, Is.Null);

            Assert.That(type1Routes, Is.TypeOf<SubscriptionRouteCollection>());
            Assert.That(type2Routes, Is.TypeOf<SubscriptionRouteCollection>());
            Assert.That(type1Routes!.DeserializationType, Is.EqualTo(typeof(string)));
            Assert.That(type2Routes!.DeserializationType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void Handle_Should_InvokeRoutesWithoutTopicFilter_WhenTopicFilterIsNull()
        {
            // arrange
            var calls = new List<string>();
            var collection = new SubscriptionRouteCollection(typeof(string));
            collection.AddRoute(null, MessageRoute<string>.CreateWithoutTopicFilter("type", (_, _, _, _) =>
            {
                calls.Add("no-topic");
                return null;
            }));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("topic");
                return null;
            }));
            collection.Build();

            // act
            var handled = collection.Handle(null, null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.True);
            Assert.That(result, Is.SameAs(CallResult.SuccessResult));
            Assert.That(calls, Is.EqualTo(new[] { "no-topic" }));
        }

        [Test]
        public void Handle_Should_ReturnFalse_WhenNoRoutesMatch()
        {
            // arrange
            var collection = new SubscriptionRouteCollection(typeof(string));
            collection.AddRoute("other-topic", MessageRoute<string>.CreateWithTopicFilter("type", "other-topic", (_, _, _, _) => null));
            collection.Build();

            // act
            var handled = collection.Handle("topic", null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.False);
            Assert.That(result, Is.SameAs(CallResult.SuccessResult));
        }

        [Test]
        public void Handle_Should_InvokeRoutesWithoutTopicFilter_AndMatchingTopicRoutes()
        {
            // arrange
            var calls = new List<string>();
            var collection = new SubscriptionRouteCollection(typeof(string));
            collection.AddRoute(null, MessageRoute<string>.CreateWithoutTopicFilter("type", (_, _, _, _) =>
            {
                calls.Add("no-topic");
                return null;
            }));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("topic");
                return null;
            }));
            collection.Build();

            // act
            var handled = collection.Handle("topic", null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.True);
            Assert.That(result, Is.SameAs(CallResult.SuccessResult));
            Assert.That(calls, Is.EqualTo(new[] { "no-topic", "topic" }));
        }

        [Test]
        public void Handle_Should_InvokeAllMatchingTopicRoutes()
        {
            // arrange
            var calls = new List<string>();
            var collection = new SubscriptionRouteCollection(typeof(string));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("first");
                return CallResult.SuccessResult;
            }));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("second");
                return null;
            }));
            collection.Build();

            // act
            var handled = collection.Handle("topic", null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.True);
            Assert.That(result, Is.SameAs(CallResult.SuccessResult));
            Assert.That(calls, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void Handle_Should_NotInvokeTopicRoutes_WhenTopicFilterIsNull()
        {
            // arrange
            var calls = new List<string>();
            var collection = new SubscriptionRouteCollection(typeof(string));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("topic");
                return null;
            }));
            collection.Build();

            // act
            var handled = collection.Handle(null, null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.False);
            Assert.That(result, Is.SameAs(CallResult.SuccessResult));
            Assert.That(calls, Is.Empty);
        }
    }
}