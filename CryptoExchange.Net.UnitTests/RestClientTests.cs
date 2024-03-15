﻿using CryptoExchange.Net.Authentication;
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
            options.Api1Options.RateLimiters = new List<IRateLimiter> { new RateLimiter() };
            options.Api1Options.RateLimitingBehaviour = RateLimitingBehaviour.Fail;
            options.RequestTimeout = TimeSpan.FromMinutes(1);
            var client = new TestBaseClient(options);

            // assert
            Assert.That(((TestClientOptions)client.ClientOptions).Api1Options.RateLimiters.Count == 1);
            Assert.That(((TestClientOptions)client.ClientOptions).Api1Options.RateLimitingBehaviour == RateLimitingBehaviour.Fail);
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
            var rateLimiter = new RateLimiter();
            rateLimiter.AddPartialEndpointLimit("/sapi/", requests, TimeSpan.FromSeconds(perSeconds));

            for (var i = 0; i < requests + 1; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), "/sapi/v1/system/status", HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);             
                Assert.That(i == requests? result1.Data > 1 : result1.Data == 0);
            }

            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.LimitRequestAsync(new TraceLogger(), "/sapi/v1/system/status", HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.That(result2.Data == 0);
        }

        [TestCase("/sapi/test1", true)]
        [TestCase("/sapi/test2", true)]
        [TestCase("/api/test1", false)]
        [TestCase("sapi/test1", false)]
        [TestCase("/sapi/", true)]
        public async Task PartialEndpointRateLimiterEndpoints(string endpoint, bool expectLimiting)
        {
            var rateLimiter = new RateLimiter();
            rateLimiter.AddPartialEndpointLimit("/sapi/", 1, TimeSpan.FromSeconds(0.1));

            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
                bool expected = i == 1 ? (expectLimiting ? result1.Data > 1 : result1.Data == 0) : result1.Data == 0; 
                Assert.That(expected);
            }
        }
        [TestCase("/sapi/", "/sapi/", true)]
        [TestCase("/sapi/test", "/sapi/test", true)]
        [TestCase("/sapi/test", "/sapi/test123", false)]
        [TestCase("/sapi/test", "/sapi/", false)]
        public async Task PartialEndpointRateLimiterEndpoints(string endpoint1, string endpoint2, bool expectLimiting)
        {
            var rateLimiter = new RateLimiter();
            rateLimiter.AddPartialEndpointLimit("/sapi/", 1, TimeSpan.FromSeconds(0.1), countPerEndpoint: true);

            var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint1, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            var result2 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint2, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.That(result1.Data == 0);            
            Assert.That(expectLimiting ? result2.Data > 0 : result2.Data == 0);            
        }

        [TestCase(1, 0.1)]
        [TestCase(2, 0.1)]
        [TestCase(5, 1)]
        [TestCase(1, 2)]
        public async Task EndpointRateLimiterBasics(int requests, double perSeconds)
        {
            var rateLimiter = new RateLimiter();
            rateLimiter.AddEndpointLimit("/sapi/test", requests, TimeSpan.FromSeconds(perSeconds));

            for (var i = 0; i < requests + 1; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), "/sapi/test", HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
                Assert.That(i == requests ? result1.Data > 1 : result1.Data == 0);
            }

            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.LimitRequestAsync(new TraceLogger(), "/sapi/test", HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.That(result2.Data == 0);
        }

        [TestCase("/", false)]
        [TestCase("/sapi/test", true)]
        [TestCase("/sapi/test/123", false)]
        public async Task EndpointRateLimiterEndpoints(string endpoint, bool expectLimited)
        {
            var rateLimiter = new RateLimiter();
            rateLimiter.AddEndpointLimit("/sapi/test", 1, TimeSpan.FromSeconds(0.1));

            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
                bool expected = i == 1 ? (expectLimited ? result1.Data > 1 : result1.Data == 0) : result1.Data == 0; 
                Assert.That(expected);
            }
        }

        [TestCase("/", false)]
        [TestCase("/sapi/test", true)]
        [TestCase("/sapi/test2", true)]
        [TestCase("/sapi/test23", false)]
        public async Task EndpointRateLimiterMultipleEndpoints(string endpoint, bool expectLimited)
        {
            var rateLimiter = new RateLimiter();
            rateLimiter.AddEndpointLimit(new[] { "/sapi/test", "/sapi/test2" }, 1, TimeSpan.FromSeconds(0.1));

            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
                bool expected = i == 1 ? (expectLimited ? result1.Data > 1 : result1.Data == 0) : result1.Data == 0;
                Assert.That(expected);
            }
        }

        [TestCase("123", "123", "/sapi/test", "/sapi/test", true, true, true, true)]
        [TestCase("123", "456", "/sapi/test", "/sapi/test", true, true, true, false)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test2", true, true, true, true)]
        [TestCase("123", "123", "/sapi/test2", "/sapi/test", true, true, true, true)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test", true, false, true, false)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test", false, true, true, false)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test", false, false, true, false)]
        [TestCase(null, "123", "/sapi/test", "/sapi/test", false, true, true, false)]
        [TestCase("123", null, "/sapi/test", "/sapi/test", true, false, true, false)]
        [TestCase(null, null, "/sapi/test", "/sapi/test", false, false, true, false)]

        [TestCase("123", "123", "/sapi/test", "/sapi/test", true, true, false, true)]
        [TestCase("123", "456", "/sapi/test", "/sapi/test", true, true, false, false)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test2", true, true, false, true)]
        [TestCase("123", "123", "/sapi/test2", "/sapi/test", true, true, false, true)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test", true, false, false, true)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test", false, true, false, true)]
        [TestCase("123", "123", "/sapi/test", "/sapi/test", false, false, false, true)]
        [TestCase(null, "123", "/sapi/test", "/sapi/test", false, true, false, false)]
        [TestCase("123", null, "/sapi/test", "/sapi/test", true, false, false, false)]
        [TestCase(null, null, "/sapi/test", "/sapi/test", false, false, false, true)]
        public async Task ApiKeyRateLimiterBasics(string key1, string key2, string endpoint1, string endpoint2, bool signed1, bool signed2, bool onlyForSignedRequests, bool expectLimited)
        {
            var rateLimiter = new RateLimiter();
            rateLimiter.AddApiKeyLimit(1, TimeSpan.FromSeconds(0.1), onlyForSignedRequests, false);

            var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint1, HttpMethod.Get, signed1, key1?.ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            var result2 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint2, HttpMethod.Get, signed2, key2?.ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.That(result1.Data == 0);
            Assert.That(expectLimited ? result2.Data > 0 : result2.Data == 0);            
        }

        [TestCase("/sapi/test", "/sapi/test", true)]
        [TestCase("/sapi/test1", "/api/test2", true)]
        [TestCase("/", "/sapi/test2", true)]
        public async Task TotalRateLimiterBasics(string endpoint1, string endpoint2, bool expectLimited)
        {
            var rateLimiter = new RateLimiter();
            rateLimiter.AddTotalRateLimit(1, TimeSpan.FromSeconds(0.1));

            var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint1, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            var result2 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint2, HttpMethod.Get, true, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.That(result1.Data == 0);
            Assert.That(expectLimited ? result2.Data > 0 : result2.Data == 0);
        }

        [TestCase("/sapi/test", true, true, true, false)]
        [TestCase("/sapi/test", false, true, true, false)]
        [TestCase("/sapi/test", false, true, false, true)]
        [TestCase("/sapi/test", true, true, false, true)]
        public async Task ApiKeyRateLimiterIgnores_TotalRateLimiter_IfSet(string endpoint, bool signed1, bool signed2, bool ignoreTotal, bool expectLimited)
        {
            var rateLimiter = new RateLimiter();
            rateLimiter.AddApiKeyLimit(100, TimeSpan.FromSeconds(0.1), true, ignoreTotal);
            rateLimiter.AddTotalRateLimit(1, TimeSpan.FromSeconds(0.1));

            var result1 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint, HttpMethod.Get, signed1, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            var result2 = await rateLimiter.LimitRequestAsync(new TraceLogger(), endpoint, HttpMethod.Get, signed2, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.That(result1.Data == 0);
            Assert.That(expectLimited ? result2.Data > 0 : result2.Data == 0);
        }
    }
}
