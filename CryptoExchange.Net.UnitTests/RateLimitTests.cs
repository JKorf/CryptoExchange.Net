using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.RateLimiting.Trackers;
using CryptoExchange.Net.UnitTests.Implementations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class RateLimitTests
    {
        [TestCase(1, 0.1)]
        [TestCase(2, 0.1)]
        [TestCase(5, 1)]
        [TestCase(1, 2)]
        public async Task PartialEndpointRateLimiterBasics(int requests, double perSeconds)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new PathStartFilter("/sapi/"), requests, TimeSpan.FromSeconds(perSeconds), RateLimitWindowType.Fixed));

            var triggered = false;
            rateLimiter.RateLimitTriggered += (x) => { triggered = true; };
            var requestDefinition = new RequestDefinition("https://test.com", "/sapi/v1/system/status", HttpMethod.Get);

            for (var i = 0; i < requests + 1; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
                Assert.That(i == requests ? triggered : !triggered);
            }
            triggered = false;
            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(!triggered);
        }

        [TestCase("/sapi/test1", true)]
        [TestCase("/sapi/test2", true)]
        [TestCase("/api/test1", false)]
        [TestCase("sapi/test1", true)]
        [TestCase("/sapi/", true)]
        public async Task PartialEndpointRateLimiterEndpoints(string endpoint, bool expectLimiting)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new PathStartFilter("/sapi/"), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));

            var requestDefinition = new RequestDefinition("https://test.com", endpoint, HttpMethod.Get);

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
                bool expected = i == 1 ? expectLimiting ? evnt?.DelayTime > TimeSpan.Zero : evnt == null : evnt == null;
                Assert.That(expected);
            }
        }

        [TestCase("/sapi/", "/sapi/", true)]
        [TestCase("/sapi/test", "/sapi/test", true)]
        [TestCase("/sapi/test", "/sapi/test123", false)]
        [TestCase("/sapi/test", "/sapi/", false)]
        public async Task PartialEndpointRateLimiterEndpoints(string endpoint1, string endpoint2, bool expectLimiting)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerEndpoint, new PathStartFilter("/sapi/"), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));

            var requestDefinition1 = new RequestDefinition("https://test.com", endpoint1, HttpMethod.Get);
            var requestDefinition2 = new RequestDefinition("https://test.com", endpoint2, HttpMethod.Get);

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(expectLimiting ? evnt != null : evnt == null);
        }

        [TestCase(1, 0.1)]
        [TestCase(2, 0.1)]
        [TestCase(5, 1)]
        [TestCase(1, 2)]
        public async Task EndpointRateLimiterBasics(int requests, double perSeconds)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerEndpoint, new PathStartFilter("/sapi/test"), requests, TimeSpan.FromSeconds(perSeconds), RateLimitWindowType.Fixed));

            bool triggered = false;
            rateLimiter.RateLimitTriggered += (x) => { triggered = true; };
            var requestDefinition = new RequestDefinition("https://test.com", "/sapi/test", HttpMethod.Get);

            for (var i = 0; i < requests + 1; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
                Assert.That(i == requests ? triggered : !triggered);
            }
            triggered = false;
            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(!triggered);
        }

        [TestCase("/", false)]
        [TestCase("/sapi/test", true)]
        [TestCase("/sapi/test/123", false)]
        public async Task EndpointRateLimiterEndpoints(string endpoint, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerEndpoint, new ExactPathFilter("/sapi/test"), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));

            var requestDefinition = new RequestDefinition("https://test.com", endpoint, HttpMethod.Get);

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
                bool expected = i == 1 ? expectLimited ? evnt?.DelayTime > TimeSpan.Zero : evnt == null : evnt == null;
                Assert.That(expected);
            }
        }

        [TestCase("/", false)]
        [TestCase("/sapi/test", true)]
        [TestCase("/sapi/test2", true)]
        [TestCase("/sapi/test23", false)]
        public async Task EndpointRateLimiterMultipleEndpoints(string endpoint, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerEndpoint, new ExactPathsFilter(new[] { "/sapi/test", "/sapi/test2" }), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));
            var requestDefinition = new RequestDefinition("https://test.com", endpoint, HttpMethod.Get);

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
                bool expected = i == 1 ? expectLimited ? evnt?.DelayTime > TimeSpan.Zero : evnt == null : evnt == null;
                Assert.That(expected);
            }
        }

        [TestCase("123", "123", "/sapi/test", "/sapi/test", true)]
        [TestCase("123", "456", "/sapi/test", "/sapi/test", false)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test2", true)]
        [TestCase("123", "123", "/sapi/test2", "/sapi/test", true)]
        [TestCase(null, "123", "/sapi/test", "/sapi/test", false)]
        [TestCase("123", null, "/sapi/test", "/sapi/test", false)]
        [TestCase(null, null, "/sapi/test", "/sapi/test", false)]
        public async Task ApiKeyRateLimiterBasics(string key1, string key2, string endpoint1, string endpoint2, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerApiKey, new AuthenticatedEndpointFilter(true), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Sliding));
            var requestDefinition1 = new RequestDefinition("https://test.com", endpoint1, HttpMethod.Get) { Authenticated = key1 != null };
            var requestDefinition2 = new RequestDefinition("https://test.com", endpoint2, HttpMethod.Get) { Authenticated = key2 != null };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, key1, 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, key2, 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(expectLimited ? evnt != null : evnt == null);
        }

        [TestCase("/sapi/test", "/sapi/test", true)]
        [TestCase("/sapi/test1", "/api/test2", true)]
        [TestCase("/", "/sapi/test2", true)]
        public async Task TotalRateLimiterBasics(string endpoint1, string endpoint2, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, Array.Empty<IGuardFilter>(), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));
            var requestDefinition1 = new RequestDefinition("https://test.com", endpoint1, HttpMethod.Get);
            var requestDefinition2 = new RequestDefinition("https://test.com", endpoint2, HttpMethod.Get) { Authenticated = true };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, null, 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(expectLimited ? evnt != null : evnt == null);
        }

        [TestCase("https://test.com", "/sapi/test", "https://test.com", "/sapi/test", true)]
        [TestCase("https://test2.com", "/sapi/test", "https://test.com", "/sapi/test", false)]
        [TestCase("https://test.com", "/sapi/test", "https://test2.com", "/sapi/test", false)]
        [TestCase("https://test.com", "/sapi/test", "https://test.com", "/sapi/test2", true)]
        public async Task HostRateLimiterBasics(string host1, string endpoint1, string host2, string endpoint2, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new HostFilter("https://test.com"), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));
            var requestDefinition1 = new RequestDefinition(host1, endpoint1, HttpMethod.Get);
            var requestDefinition2 = new RequestDefinition(host2, endpoint2, HttpMethod.Get) { Authenticated = true };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(expectLimited ? evnt != null : evnt == null);
        }

        [TestCase("https://test.com", "https://test.com", true)]
        [TestCase("https://test2.com", "https://test.com", false)]
        [TestCase("https://test.com", "https://test2.com", false)]
        public async Task ConnectionRateLimiterBasics(string host1, string host2, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new LimitItemTypeFilter(RateLimitItemType.Connection), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition(host1, "1", HttpMethod.Get), "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition(host2, "1", HttpMethod.Get), "123", 1, RateLimitingBehaviour.Wait, null, 1, default);
            Assert.That(expectLimited ? evnt != null : evnt == null);
        }

        [Test]
        public async Task ConnectionRateLimiterCancel()
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new LimitItemTypeFilter(RateLimitItemType.Connection), 1, TimeSpan.FromSeconds(10), RateLimitWindowType.Fixed));

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(0.2));

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("https://test.com", "1", HttpMethod.Get), "123", 1, RateLimitingBehaviour.Wait, null, 1, ct.Token);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("https://test.com", "1", HttpMethod.Get), "123", 1, RateLimitingBehaviour.Wait, null, 1, ct.Token);
            Assert.That(result2.Error, Is.TypeOf<CancellationRequestedError>());
        }

        [Test]
        public async Task RateLimiterReset_Should_AllowNextRequestForSameDefinition()
        {
            // arrange
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerConnection, new LimitItemTypeFilter(RateLimitItemType.Request), 1, TimeSpan.FromSeconds(10), RateLimitWindowType.Fixed));

            var definition = new RequestDefinition("https://test.com", "1", HttpMethod.Get) { ConnectionId = 1 };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(0.2));

            // act
            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition, null, 1, RateLimitingBehaviour.Fail, null, 1, ct.Token);
            await rateLimiter.ResetAsync(RateLimitItemType.Request, definition, null, null, null, default);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition, null, 1, RateLimitingBehaviour.Fail, null, 1, ct.Token);
            
            // assert
            Assert.That(evnt, Is.Null);
        }

        [Test]
        public async Task RateLimiterReset_Should_NotAllowNextRequestForDifferentDefinition()
        {
            // arrange
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerConnection, new LimitItemTypeFilter(RateLimitItemType.Request), 1, TimeSpan.FromSeconds(10), RateLimitWindowType.Fixed));

            var definition1 = new RequestDefinition("https://test.com", "1", HttpMethod.Get) { ConnectionId = 1 };
            var definition2 = new RequestDefinition("https://test.com", "2", HttpMethod.Get) { ConnectionId = 2 };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            // act
            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition1, null, 1, RateLimitingBehaviour.Fail, null, 1, default);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition2, null, 1, RateLimitingBehaviour.Fail, null, 1, default);
            await rateLimiter.ResetAsync(RateLimitItemType.Request, definition1, null, null, null, default);
            var result3 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition2, null, 1, RateLimitingBehaviour.Fail, null, 1, default);
            
            // assert
            Assert.That(evnt, Is.Not.Null);
        }

        [TestCase(null, null, true)]
        [TestCase("Group1", null, false)]
        [TestCase(null, "Group2", false)]
        [TestCase("Group1", "Group2", false)]
        [TestCase("Group3", "Group3", true)]
        public async Task RateLimiterWithDifferentGroups_Should_LimitPerGroup(string? group1, string? group2, bool expectLimited)
        {
            // arrange
            var data = JsonSerializer.Serialize(new TestObject { });
            var client1 = new TestRestClient(x =>
            {
                x.RateLimitGroup = group1;
            });
            client1.ApiClient1.SetNextResponse(data, System.Net.HttpStatusCode.OK);
            var client2 = new TestRestClient(x =>
            {
                x.RateLimitGroup = group2;
            });
            client2.ApiClient1.SetNextResponse(data, System.Net.HttpStatusCode.OK);

            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new LimitItemTypeFilter(RateLimitItemType.Request), 1, TimeSpan.FromSeconds(2), RateLimitWindowType.Fixed));

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            // act
            var result1 = await client1.ApiClient1.GetResponseAsync<TestObject>(rateLimitGate: rateLimiter);
            var result2 = await client2.ApiClient1.GetResponseAsync<TestObject>(rateLimitGate: rateLimiter);

            // assert
            Assert.That(evnt != null, Is.EqualTo(expectLimited));
        }

        [TestCase(RateLimitWindowType.Fixed)]
        [TestCase(RateLimitWindowType.FixedAfterFirst)]
        [TestCase(RateLimitWindowType.Sliding)]
        [TestCase(RateLimitWindowType.Decay)]
        public async Task LowerThreshold_ReservesCapacityForFullThreshold(RateLimitWindowType windowType)
        {
            var gate = new RateLimitGate("Test");
            gate.AddGuard(new RateLimitGuard(
                RateLimitGuard.PerHost,
                new LimitItemTypeFilter(RateLimitItemType.Request),
                10,
                TimeSpan.FromHours(1),
                windowType,
                decayPerTimeSpan: 1));

            var definition = new RequestDefinition(
                "https://test.com", "/ticker", HttpMethod.Get);
            var logger = new TraceLogger();

            for (var i = 0; i < 8; i++)
            {
                var result = await gate.ProcessAsync(
                    logger, i, RateLimitItemType.Request, definition, null, 1,
                    RateLimitingBehaviour.Fail, null, 0.8, default);
                Assert.That(result.Success, Is.True);
            }

            var marketData = await gate.ProcessAsync(
                logger, 9, RateLimitItemType.Request, definition, null, 1,
                RateLimitingBehaviour.Fail, null, 0.8, default);
            Assert.That(marketData.Error, Is.TypeOf<ClientRateLimitError>());

            var order = await gate.ProcessAsync(
                logger, 10, RateLimitItemType.Request, definition, null, 2,
                RateLimitingBehaviour.Fail, null, 1.0, default);
            Assert.That(order.Success, Is.True);

            var overHardLimit = await gate.ProcessAsync(
                logger, 11, RateLimitItemType.Request, definition, null, 1,
                RateLimitingBehaviour.Fail, null, 1.0, default);
            Assert.That(overHardLimit.Error, Is.TypeOf<ClientRateLimitError>());
        }

        [TestCase(0.1, 10)]
        [TestCase(1, 50)]
        [TestCase(5, 250)]
        [TestCase(60, 250)]
        public void RateLimitSafetyMargin_DefaultIsProportionalAndCapped(double periodSeconds, int expectedMarginMilliseconds)
        {
            var margin = WindowTrackerHelpers.GetDefaultSafetyMargin(TimeSpan.FromSeconds(periodSeconds));

            Assert.That(margin, Is.EqualTo(TimeSpan.FromMilliseconds(expectedMarginMilliseconds)));
        }

        [Test]
        public void RateLimitGuard_ExplicitSafetyMarginIsUsed()
        {
            var guard = new RateLimitGuard(
                RateLimitGuard.PerHost,
                new LimitItemTypeFilter(RateLimitItemType.Request),
                1,
                TimeSpan.FromSeconds(1),
                RateLimitWindowType.Sliding,
                safetyMargin: TimeSpan.Zero);

            Assert.That(guard.SafetyMargin, Is.EqualTo(TimeSpan.Zero));
        }
    }
}
