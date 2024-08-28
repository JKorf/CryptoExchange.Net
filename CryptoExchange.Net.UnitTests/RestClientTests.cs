using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CryptoExchange.Net.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using NUnit.Framework.Legacy;
using CryptoExchange.Net.RateLimiting;
using System.Net;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class RestClientTests
    {
        [TestCase]
        public void RequestingData_Should_ResultInData()
        {
            // arrange
            var client = new TestRestClient();
            var expected = new TestObject() { DecimalData = 1.23M, IntData = 10, StringData = "Some data" };
            client.SetResponse(JsonConvert.SerializeObject(expected), out _);

            // act
            var result = client.Api1.Request<TestObject>().Result;

            // assert
            Assert.That(result.Success);
            Assert.That(TestHelpers.AreEqual(expected, result.Data));
        }

        [TestCase]
        public void ReceivingInvalidData_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetResponse("{\"property\": 123", out _);

            // act
            var result = client.Api1.Request<TestObject>().Result;

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
        }

        [TestCase]
        public async Task ReceivingErrorCode_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetErrorWithoutResponse(System.Net.HttpStatusCode.BadRequest, "Invalid request");

            // act
            var result = await client.Api1.Request<TestObject>();

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
        }

        [TestCase]
        public async Task ReceivingErrorAndNotParsingError_Should_ResultInFlatError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetErrorWithResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = await client.Api1.Request<TestObject>();

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
            Assert.That(result.Error is ServerError);
            Assert.That(result.Error.Message.Contains("Invalid request"));
            Assert.That(result.Error.Message.Contains("123"));
        }

        [TestCase]
        public async Task ReceivingErrorAndParsingError_Should_ResultInParsedError()
        {
            // arrange
            var client = new ParseErrorTestRestClient();
            client.SetErrorWithResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = await client.Api2.Request<TestObject>();

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
            Assert.That(result.Error is ServerError);
            Assert.That(result.Error.Code == 123);
            Assert.That(result.Error.Message == "Invalid request");
        }

        [TestCase]
        public void SettingOptions_Should_ResultInOptionsSet()
        {
            // arrange
            // act
            var options = new TestClientOptions();
            options.Api1Options.TimestampRecalculationInterval = TimeSpan.FromMinutes(10);
            options.Api1Options.OutputOriginalData = true;
            options.RequestTimeout = TimeSpan.FromMinutes(1);
            var client = new TestBaseClient(options);

            // assert
            Assert.That(((TestClientOptions)client.ClientOptions).Api1Options.TimestampRecalculationInterval == TimeSpan.FromMinutes(10));
            Assert.That(((TestClientOptions)client.ClientOptions).Api1Options.OutputOriginalData == true);
            Assert.That(((TestClientOptions)client.ClientOptions).RequestTimeout == TimeSpan.FromMinutes(1));
        }

        [TestCase("GET", HttpMethodParameterPosition.InUri)] // No need to test InBody for GET since thats not valid
        [TestCase("POST", HttpMethodParameterPosition.InBody)]
        [TestCase("POST", HttpMethodParameterPosition.InUri)]
        [TestCase("DELETE", HttpMethodParameterPosition.InBody)]
        [TestCase("DELETE", HttpMethodParameterPosition.InUri)]
        [TestCase("PUT", HttpMethodParameterPosition.InUri)]
        [TestCase("PUT", HttpMethodParameterPosition.InBody)]
        public async Task Setting_Should_ResultInOptionsSet(string method, HttpMethodParameterPosition pos)
        {
            // arrange
            // act
            var client = new TestRestClient();

            client.Api1.SetParameterPosition(new HttpMethod(method), pos);

            client.SetResponse("{}", out var request);

            await client.Api1.RequestWithParams<TestObject>(new HttpMethod(method), new Dictionary<string, object>
            {
                { "TestParam1", "Value1" },
                { "TestParam2", 2 },
            },
            new Dictionary<string, string>
            {
                { "TestHeader", "123" }
            });

            // assert
            Assert.That(request.Method == new HttpMethod(method));
            Assert.That((request.Content?.Contains("TestParam1") == true) == (pos == HttpMethodParameterPosition.InBody));
            Assert.That((request.Uri.ToString().Contains("TestParam1")) == (pos == HttpMethodParameterPosition.InUri));
            Assert.That((request.Content?.Contains("TestParam2") == true) == (pos == HttpMethodParameterPosition.InBody));
            Assert.That((request.Uri.ToString().Contains("TestParam2")) == (pos == HttpMethodParameterPosition.InUri));
            Assert.That(request.GetHeaders().First().Key == "TestHeader");
            Assert.That(request.GetHeaders().First().Value.Contains("123"));
        }


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
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);             
                Assert.That(i == requests? triggered : !triggered);
            }
            triggered = false;
            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1,  RateLimitingBehaviour.Wait, default);
            Assert.That(!triggered);
        }

        [TestCase("/sapi/test1", true)]
        [TestCase("/sapi/test2", true)]
        [TestCase("/api/test1", false)]
        [TestCase("sapi/test1", false)]
        [TestCase("/sapi/", true)]
        public async Task PartialEndpointRateLimiterEndpoints(string endpoint, bool expectLimiting)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new PathStartFilter("/sapi/"), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));

            var requestDefinition = new RequestDefinition(endpoint, HttpMethod.Get);

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);
                bool expected = i == 1 ? (expectLimiting ? evnt.DelayTime > TimeSpan.Zero : evnt == null) : evnt == null;
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

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);
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
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);
                Assert.That(i == requests ? triggered : !triggered);
            }
            triggered = false;
            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);
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

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);
                bool expected = i == 1 ? (expectLimited ? evnt.DelayTime > TimeSpan.Zero : evnt == null) : evnt == null;
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

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);
                bool expected = i == 1 ? (expectLimited ? evnt.DelayTime > TimeSpan.Zero : evnt == null) : evnt == null;
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
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerApiKey, new AuthenticatedEndpointFilter(true), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));
            var requestDefinition1 = new RequestDefinition(endpoint1, HttpMethod.Get) { Authenticated = key1 != null };
            var requestDefinition2 = new RequestDefinition(endpoint2, HttpMethod.Get) { Authenticated = key2 != null };

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "https://test.com", key1, 1, RateLimitingBehaviour.Wait, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, "https://test.com", key2, 1, RateLimitingBehaviour.Wait, default);
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

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, "https://test.com", "123", 1, RateLimitingBehaviour.Wait, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition2, "https://test.com", null, 1, RateLimitingBehaviour.Wait, default);
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

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, host1, "123", 1, RateLimitingBehaviour.Wait, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Request, requestDefinition1, host2, "123", 1, RateLimitingBehaviour.Wait, default);
            Assert.That(expectLimited ? evnt != null : evnt == null);
        }

        [TestCase("https://test.com", "https://test.com", true)]
        [TestCase("https://test2.com", "https://test.com", false)]
        [TestCase("https://test.com", "https://test2.com", false)]
        public async Task ConnectionRateLimiterBasics(string host1, string host2, bool expectLimited)
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new LimitItemTypeFilter(RateLimitItemType.Connection), 1, TimeSpan.FromSeconds(0.1), RateLimitWindowType.Fixed));

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("1", HttpMethod.Get), host1, "123", 1, RateLimitingBehaviour.Wait, default);
            Assert.That(evnt == null);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("1", HttpMethod.Get), host2, "123", 1, RateLimitingBehaviour.Wait, default);
            Assert.That(expectLimited ? evnt != null : evnt == null);
        }

        [Test]
        public async Task ConnectionRateLimiterCancel()
        {
            var rateLimiter = new RateLimitGate("Test");
            rateLimiter.AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new LimitItemTypeFilter(RateLimitItemType.Connection), 1, TimeSpan.FromSeconds(10), RateLimitWindowType.Fixed));

            RateLimitEvent evnt = null;
            rateLimiter.RateLimitTriggered += (x) => { evnt = x; };
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(0.2));

            var result1 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("1", HttpMethod.Get), "https://test.com", "123", 1, RateLimitingBehaviour.Wait, ct.Token);
            var result2 = await rateLimiter.ProcessAsync(new TraceLogger(), 1, RateLimitItemType.Connection, new RequestDefinition("1", HttpMethod.Get), "https://test.com", "123", 1, RateLimitingBehaviour.Wait, ct.Token);
            Assert.That(result2.Error, Is.TypeOf<CancellationRequestedError>());
        }
    }
}
