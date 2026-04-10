using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
            var requestDefinition = new RequestDefinition("/sapi/v1/system/status", HttpMethod.Get);

            for (var i = 0; i < requests + 1; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
                Assert.That(i == requests ? triggered : !triggered);
            }
            triggered = false;
            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
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

            var requestDefinition = new RequestDefinition(endpoint, HttpMethod.Get);

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
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

            var requestDefinition1 = new RequestDefinition(endpoint1, HttpMethod.Get);
            var requestDefinition2 = new RequestDefinition(endpoint2, HttpMethod.Get);

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
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
            var requestDefinition = new RequestDefinition("/sapi/test", HttpMethod.Get);

            for (var i = 0; i < requests + 1; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
                Assert.That(i == requests ? triggered : !triggered);
            }
            triggered = false;
            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
            Assert.That(!triggered);
        }

        [TestCase("/", false)]
        [TestCase("/sapi/test", true)]
        [TestCase("/sapi/test/123", false)]
        public async Task EndpointRateLimiterEndpoints(string endpoint, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerEndpoint, new ExactPathFilter("/sapi/test"), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));

            var requestDefinition = new RequestDefinition(endpoint, HttpMethod.Get);

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
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
            var requestDefinition = new RequestDefinition(endpoint, HttpMethod.Get);

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
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
            var requestDefinition1 = new RequestDefinition(endpoint1, HttpMethod.Get) { Authenticated = key1 != null };
            var requestDefinition2 = new RequestDefinition(endpoint2, HttpMethod.Get) { Authenticated = key2 != null };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "https://test.com", key1, 1, RateLimitingBehaviour.Wait, null, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, "https://test.com", key2, 1, RateLimitingBehaviour.Wait, null, default);
            Assert.That(expectLimited ? evnt != null : evnt == null);
        }

        [TestCase("/sapi/test", "/sapi/test", true)]
        [TestCase("/sapi/test1", "/api/test2", true)]
        [TestCase("/", "/sapi/test2", true)]
        public async Task TotalRateLimiterBasics(string endpoint1, string endpoint2, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, Array.Empty<IGuardFilter>(), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));
            var requestDefinition1 = new RequestDefinition(endpoint1, HttpMethod.Get);
            var requestDefinition2 = new RequestDefinition(endpoint2, HttpMethod.Get) { Authenticated = true };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, "https://test.com", null, 1, RateLimitingBehaviour.Wait, null, default);
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
            var requestDefinition1 = new RequestDefinition(endpoint1, HttpMethod.Get);
            var requestDefinition2 = new RequestDefinition(endpoint2, HttpMethod.Get) { Authenticated = true };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, host1, "123", 1, RateLimitingBehaviour.Wait, null, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, host2, "123", 1, RateLimitingBehaviour.Wait, null, default);
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

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("1", HttpMethod.Get), host1, "123", 1, RateLimitingBehaviour.Wait, null, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("1", HttpMethod.Get), host2, "123", 1, RateLimitingBehaviour.Wait, null, default);
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

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("1", HttpMethod.Get), "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, ct.Token);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("1", HttpMethod.Get), "https://test.com", "123", 1, RateLimitingBehaviour.Wait, null, ct.Token);
            Assert.That(result2.Error, Is.TypeOf<CancellationRequestedError>());
        }

        [Test]
        public async Task RateLimiterReset_Should_AllowNextRequestForSameDefinition()
        {
            // arrange
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerConnection, new LimitItemTypeFilter(RateLimitItemType.Request), 1, TimeSpan.FromSeconds(10), RateLimitWindowType.Fixed));

            var definition = new RequestDefinition("1", HttpMethod.Get) { ConnectionId = 1 };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(0.2));

            // act
            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition, "https://test.com", null, 1, RateLimitingBehaviour.Fail, null, ct.Token);
            await rateLimiter.ResetAsync(RateLimitItemType.Request, definition, "https://test.com", null, null, default);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition, "https://test.com", null, 1, RateLimitingBehaviour.Fail, null, ct.Token);
            
            // assert
            Assert.That(evnt, Is.Null);
        }

        [Test]
        public async Task RateLimiterReset_Should_NotAllowNextRequestForDifferentDefinition()
        {
            // arrange
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerConnection, new LimitItemTypeFilter(RateLimitItemType.Request), 1, TimeSpan.FromSeconds(10), RateLimitWindowType.Fixed));

            var definition1 = new RequestDefinition("1", HttpMethod.Get) { ConnectionId = 1 };
            var definition2 = new RequestDefinition("2", HttpMethod.Get) { ConnectionId = 2 };

            RateLimitEvent? evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            // act
            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition1, "https://test.com", null, 1, RateLimitingBehaviour.Fail, null, default);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition2, "https://test.com", null, 1, RateLimitingBehaviour.Fail, null, default);
            await rateLimiter.ResetAsync(RateLimitItemType.Request, definition1, "https://test.com", null, null, default);
            var result3 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, definition2, "https://test.com", null, 1, RateLimitingBehaviour.Fail, null, default);
            
            // assert
            Assert.That(evnt, Is.Not.Null);
        }
    }
}
