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
using CryptoExchange.Net.Logging;
using System.Threading;

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
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(TestHelpers.AreEqual(expected, result.Data));
        }

        [TestCase]
        public void ReceivingInvalidData_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetResponse("{\"property\": 123", out _);

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
        }

        [TestCase]
        public async Task ReceivingErrorCode_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetErrorWithoutResponse(System.Net.HttpStatusCode.BadRequest, "Invalid request");

            // act
            var result = await client.Request<TestObject>();

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
        }

        [TestCase]
        public async Task ReceivingErrorAndNotParsingError_Should_ResultInFlatError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetErrorWithResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = await client.Request<TestObject>();

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
            Assert.IsTrue(result.Error is ServerError);
            Assert.IsTrue(result.Error.Message.Contains("Invalid request"));
            Assert.IsTrue(result.Error.Message.Contains("123"));
        }

        [TestCase]
        public async Task ReceivingErrorAndParsingError_Should_ResultInParsedError()
        {
            // arrange
            var client = new ParseErrorTestRestClient();
            client.SetErrorWithResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = await client.Request<TestObject>();

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
            Assert.IsTrue(result.Error is ServerError);
            Assert.IsTrue(result.Error.Code == 123);
            Assert.IsTrue(result.Error.Message == "Invalid request");
        }

        [TestCase]
        public void SettingOptions_Should_ResultInOptionsSet()
        {
            // arrange
            // act
            var client = new TestRestClient(new TestClientOptions()
            {
                Api1Options = new RestApiClientOptions
                {
                    BaseAddress = "http://test.address.com",
                    RateLimiters = new List<IRateLimiter> { new RateLimiter() },
                    RateLimitingBehaviour = RateLimitingBehaviour.Fail
                },
                RequestTimeout = TimeSpan.FromMinutes(1)
            });


            // assert
            Assert.IsTrue(((TestClientOptions)client.ClientOptions).Api1Options.BaseAddress == "http://test.address.com");
            Assert.IsTrue(((TestClientOptions)client.ClientOptions).Api1Options.RateLimiters.Count == 1);
            Assert.IsTrue(((TestClientOptions)client.ClientOptions).Api1Options.RateLimitingBehaviour == RateLimitingBehaviour.Fail);
            Assert.IsTrue(client.ClientOptions.RequestTimeout == TimeSpan.FromMinutes(1));
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
            var client = new TestRestClient(new TestClientOptions()
            {
                Api1Options = new RestApiClientOptions
                {
                    BaseAddress = "http://test.address.com"
                }
            });

            client.SetParameterPosition(new HttpMethod(method), pos);

            client.SetResponse("{}", out var request);

            await client.RequestWithParams<TestObject>(new HttpMethod(method), new Dictionary<string, object>
            {
                { "TestParam1", "Value1" },
                { "TestParam2", 2 },
            },
            new Dictionary<string, string>
            {
                { "TestHeader", "123" }
            });

            // assert
            Assert.AreEqual(request.Method, new HttpMethod(method));
            Assert.AreEqual(request.Content?.Contains("TestParam1") == true, pos == HttpMethodParameterPosition.InBody);
            Assert.AreEqual(request.Uri.ToString().Contains("TestParam1"), pos == HttpMethodParameterPosition.InUri);
            Assert.AreEqual(request.Content?.Contains("TestParam2") == true, pos == HttpMethodParameterPosition.InBody);
            Assert.AreEqual(request.Uri.ToString().Contains("TestParam2"), pos == HttpMethodParameterPosition.InUri);
            Assert.AreEqual(request.GetHeaders().First().Key, "TestHeader");
            Assert.IsTrue(request.GetHeaders().First().Value.Contains("123"));
        }


        [TestCase(1, 0.1)]
        [TestCase(2, 0.1)]
        [TestCase(5, 1)]
        [TestCase(1, 2)]
        public async Task PartialEndpointRateLimiterBasics(int requests, double perSeconds)
        {
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddPartialEndpointLimit("/sapi/", requests, TimeSpan.FromSeconds(perSeconds));

            for (var i = 0; i < requests + 1; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(log, "/sapi/v1/system/status", HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);             
                Assert.IsTrue(i == requests? result1.Data > 1 : result1.Data == 0);
            }

            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.LimitRequestAsync(log, "/sapi/v1/system/status", HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.IsTrue(result2.Data == 0);
        }

        [TestCase("/sapi/test1", true)]
        [TestCase("/sapi/test2", true)]
        [TestCase("/api/test1", false)]
        [TestCase("sapi/test1", false)]
        [TestCase("/sapi/", true)]
        public async Task PartialEndpointRateLimiterEndpoints(string endpoint, bool expectLimiting)
        {
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddPartialEndpointLimit("/sapi/", 1, TimeSpan.FromSeconds(0.1));

            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(log, endpoint, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
                bool expected = i == 1 ? (expectLimiting ? result1.Data > 1 : result1.Data == 0) : result1.Data == 0; 
                Assert.IsTrue(expected);
            }
        }
        [TestCase("/sapi/", "/sapi/", true)]
        [TestCase("/sapi/test", "/sapi/test", true)]
        [TestCase("/sapi/test", "/sapi/test123", false)]
        [TestCase("/sapi/test", "/sapi/", false)]
        public async Task PartialEndpointRateLimiterEndpoints(string endpoint1, string endpoint2, bool expectLimiting)
        {
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddPartialEndpointLimit("/sapi/", 1, TimeSpan.FromSeconds(0.1), countPerEndpoint: true);

            var result1 = await rateLimiter.LimitRequestAsync(log, endpoint1, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            var result2 = await rateLimiter.LimitRequestAsync(log, endpoint2, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.IsTrue(result1.Data == 0);            
            Assert.IsTrue(expectLimiting ? result2.Data > 0 : result2.Data == 0);            
        }

        [TestCase(1, 0.1)]
        [TestCase(2, 0.1)]
        [TestCase(5, 1)]
        [TestCase(1, 2)]
        public async Task EndpointRateLimiterBasics(int requests, double perSeconds)
        {
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddEndpointLimit("/sapi/test", requests, TimeSpan.FromSeconds(perSeconds));

            for (var i = 0; i < requests + 1; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(log, "/sapi/test", HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
                Assert.IsTrue(i == requests ? result1.Data > 1 : result1.Data == 0);
            }

            await Task.Delay((int)Math.Round(perSeconds * 1000) + 10);
            var result2 = await rateLimiter.LimitRequestAsync(log, "/sapi/test", HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.IsTrue(result2.Data == 0);
        }

        [TestCase("/", false)]
        [TestCase("/sapi/test", true)]
        [TestCase("/sapi/test/123", false)]
        public async Task EndpointRateLimiterEndpoints(string endpoint, bool expectLimited)
        {
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddEndpointLimit("/sapi/test", 1, TimeSpan.FromSeconds(0.1));

            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(log, endpoint, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
                bool expected = i == 1 ? (expectLimited ? result1.Data > 1 : result1.Data == 0) : result1.Data == 0; 
                Assert.IsTrue(expected);
            }
        }

        [TestCase("/", false)]
        [TestCase("/sapi/test", true)]
        [TestCase("/sapi/test2", true)]
        [TestCase("/sapi/test23", false)]
        public async Task EndpointRateLimiterMultipleEndpoints(string endpoint, bool expectLimited)
        {
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddEndpointLimit(new[] { "/sapi/test", "/sapi/test2" }, 1, TimeSpan.FromSeconds(0.1));

            for (var i = 0; i < 2; i++)
            {
                var result1 = await rateLimiter.LimitRequestAsync(log, endpoint, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
                bool expected = i == 1 ? (expectLimited ? result1.Data > 1 : result1.Data == 0) : result1.Data == 0;
                Assert.IsTrue(expected);
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
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddApiKeyLimit(1, TimeSpan.FromSeconds(0.1), onlyForSignedRequests, false);

            var result1 = await rateLimiter.LimitRequestAsync(log, endpoint1, HttpMethod.Get, signed1, key1?.ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            var result2 = await rateLimiter.LimitRequestAsync(log, endpoint2, HttpMethod.Get, signed2, key2?.ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.IsTrue(result1.Data == 0);
            Assert.IsTrue(expectLimited ? result2.Data > 0 : result2.Data == 0);            
        }

        [TestCase("/sapi/test", "/sapi/test", true)]
        [TestCase("/sapi/test1", "/api/test2", true)]
        [TestCase("/", "/sapi/test2", true)]
        public async Task TotalRateLimiterBasics(string endpoint1, string endpoint2, bool expectLimited)
        {
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddTotalRateLimit(1, TimeSpan.FromSeconds(0.1));

            var result1 = await rateLimiter.LimitRequestAsync(log, endpoint1, HttpMethod.Get, false, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            var result2 = await rateLimiter.LimitRequestAsync(log, endpoint2, HttpMethod.Get, true, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.IsTrue(result1.Data == 0);
            Assert.IsTrue(expectLimited ? result2.Data > 0 : result2.Data == 0);
        }

        [TestCase("/sapi/test", true, true, true, false)]
        [TestCase("/sapi/test", false, true, true, false)]
        [TestCase("/sapi/test", false, true, false, true)]
        [TestCase("/sapi/test", true, true, false, true)]
        public async Task ApiKeyRateLimiterIgnores_TotalRateLimiter_IfSet(string endpoint, bool signed1, bool signed2, bool ignoreTotal, bool expectLimited)
        {
            var log = new Log("Test");
            log.Level = LogLevel.Trace;

            var rateLimiter = new RateLimiter();
            rateLimiter.AddApiKeyLimit(100, TimeSpan.FromSeconds(0.1), true, ignoreTotal);
            rateLimiter.AddTotalRateLimit(1, TimeSpan.FromSeconds(0.1));

            var result1 = await rateLimiter.LimitRequestAsync(log, endpoint, HttpMethod.Get, signed1, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            var result2 = await rateLimiter.LimitRequestAsync(log, endpoint, HttpMethod.Get, signed2, "123".ToSecureString(), RateLimitingBehaviour.Wait, 1, default);
            Assert.IsTrue(result1.Data == 0);
            Assert.IsTrue(expectLimited ? result2.Data > 0 : result2.Data == 0);
        }
    }
}
