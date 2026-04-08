using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.UnitTests.SocketRoutingTests
{
    [TestFixture]
    public class QueryRouterTests
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

            var router = new QueryRouter(routes);

            // act
            var type1Routes = router.GetRoutes("type1");
            var type2Routes = router.GetRoutes("type2");
            var missingRoutes = router.GetRoutes("missing");

            // assert
            Assert.That(type1Routes, Is.Not.Null);
            Assert.That(type2Routes, Is.Not.Null);
            Assert.That(missingRoutes, Is.Null);

            Assert.That(type1Routes, Is.TypeOf<QueryRouteCollection>());
            Assert.That(type2Routes, Is.TypeOf<QueryRouteCollection>());
            Assert.That(type1Routes!.DeserializationType, Is.EqualTo(typeof(string)));
            Assert.That(type2Routes!.DeserializationType, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void AddRoute_Should_SetMultipleReaders_WhenAnyRouteAllowsMultipleReaders()
        {
            // arrange
            var collection = new QueryRouteCollection(typeof(string));

            // act
            collection.AddRoute(null, MessageRoute<string>.CreateWithoutTopicFilter("type", (_, _, _, _) => null));
            var beforeMultipleReaders = collection.MultipleReaders;

            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) => null, true));
            var afterMultipleReaders = collection.MultipleReaders;

            // assert
            Assert.That(beforeMultipleReaders, Is.False);
            Assert.That(afterMultipleReaders, Is.True);
        }

        [Test]
        public void Handle_Should_InvokeRoutesWithoutTopicFilter_WhenTopicFilterIsNull()
        {
            // arrange
            var calls = new List<string>();
            var collection = new QueryRouteCollection(typeof(string));
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
            Assert.That(result, Is.Null);
            Assert.That(calls, Is.EqualTo(new[] { "no-topic" }));
        }

        [Test]
        public void Handle_Should_ReturnFalse_WhenNoRoutesMatch()
        {
            // arrange
            var collection = new QueryRouteCollection(typeof(string));
            collection.AddRoute("other-topic", MessageRoute<string>.CreateWithTopicFilter("type", "other-topic", (_, _, _, _) => null));
            collection.Build();

            // act
            var handled = collection.Handle("topic", null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Handle_Should_InvokeRoutesWithoutTopicFilter_AndMatchingTopicRoutes()
        {
            // arrange
            var calls = new List<string>();
            var collection = new QueryRouteCollection(typeof(string));
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
            Assert.That(result, Is.Null);
            Assert.That(calls, Is.EqualTo(new[] { "no-topic", "topic" }));
        }

        [Test]
        public void Handle_Should_StopAfterFirstNonNullMatchingResult_WhenMultipleReadersIsFalse()
        {
            // arrange
            var calls = new List<string>();
            var expectedResult = CallResult.SuccessResult;
            var collection = new QueryRouteCollection(typeof(string));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("first");
                return expectedResult;
            }));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("second");
                return new CallResult(null);
            }));
            collection.Build();

            // act
            var handled = collection.Handle("topic", null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.True);
            Assert.That(result, Is.SameAs(expectedResult));
            Assert.That(calls, Is.EqualTo(new[] { "first" }));
        }

        [Test]
        public void Handle_Should_ContinueAfterNonNullMatchingResult_WhenMultipleReadersIsTrue()
        {
            // arrange
            var calls = new List<string>();
            var expectedResult = CallResult.SuccessResult;
            var collection = new QueryRouteCollection(typeof(string));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("first");
                return expectedResult;
            }, true));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("second");
                return new CallResult(null);
            }));
            collection.Build();

            // act
            var handled = collection.Handle("topic", null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.True);
            Assert.That(result, Is.SameAs(expectedResult));
            Assert.That(calls, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void Handle_Should_ContinueUntilNonNullResult_WhenEarlierMatchingRoutesReturnNull()
        {
            // arrange
            var calls = new List<string>();
            var expectedResult = CallResult.SuccessResult;
            var collection = new QueryRouteCollection(typeof(string));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("first");
                return null;
            }));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("second");
                return expectedResult;
            }));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) =>
            {
                calls.Add("third");
                return new CallResult(null);
            }));
            collection.Build();

            // act
            var handled = collection.Handle("topic", null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.True);
            Assert.That(result, Is.SameAs(expectedResult));
            Assert.That(calls, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void Handle_Should_ReturnHandledTrue_WhenMatchingRoutesReturnNull()
        {
            // arrange
            var collection = new QueryRouteCollection(typeof(string));
            collection.AddRoute("topic", MessageRoute<string>.CreateWithTopicFilter("type", "topic", (_, _, _, _) => null));
            collection.Build();

            // act
            var handled = collection.Handle("topic", null!, DateTime.UtcNow, "original", "data", out var result);

            // assert
            Assert.That(handled, Is.True);
            Assert.That(result, Is.Null);
        }
    }
}