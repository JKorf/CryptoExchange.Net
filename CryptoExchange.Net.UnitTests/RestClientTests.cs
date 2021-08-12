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
using CryptoExchange.Net.RateLimiter;
using Microsoft.Extensions.Logging;

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
            client.SetResponse(JsonConvert.SerializeObject(expected));

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
            client.SetResponse("{\"property\": 123");

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
        }

        [TestCase]
        public void ReceivingErrorCode_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetErrorWithoutResponse(System.Net.HttpStatusCode.BadRequest, "Invalid request");

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
        }

        [TestCase]
        public void ReceivingErrorAndNotParsingError_Should_ResultInFlatError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetErrorWithResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
            Assert.IsTrue(result.Error is ServerError);
            Assert.IsTrue(result.Error.Message.Contains("Invalid request"));
            Assert.IsTrue(result.Error.Message.Contains("123"));
        }

        [TestCase]
        public void ReceivingErrorAndParsingError_Should_ResultInParsedError()
        {
            // arrange
            var client = new ParseErrorTestRestClient();
            client.SetErrorWithResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = client.Request<TestObject>().Result;

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
            var client = new TestRestClient(new RestClientOptions("")
            {
                BaseAddress = "http://test.address.com",
                RateLimiters = new List<IRateLimiter>{new RateLimiterTotal(1, TimeSpan.FromSeconds(1))},
                RateLimitingBehaviour = RateLimitingBehaviour.Fail,
                RequestTimeout = TimeSpan.FromMinutes(1)
            });


            // assert
            Assert.IsTrue(client.BaseAddress == "http://test.address.com/");
            Assert.IsTrue(client.RateLimiters.Count() == 1);
            Assert.IsTrue(client.RateLimitBehaviour == RateLimitingBehaviour.Fail);
            Assert.IsTrue(client.RequestTimeout == TimeSpan.FromMinutes(1));
        }

        [TestCase]
        public void SettingRateLimitingBehaviourToFail_Should_FailLimitedRequests()
        {
            // arrange
            var client = new TestRestClient(new RestClientOptions("")
            {
                RateLimiters = new List<IRateLimiter> { new RateLimiterTotal(1, TimeSpan.FromSeconds(1)) },
                RateLimitingBehaviour = RateLimitingBehaviour.Fail
            });
            client.SetResponse("{\"property\": 123}");


            // act
            var result1 = client.Request<TestObject>().Result;
            client.SetResponse("{\"property\": 123}");
            var result2 = client.Request<TestObject>().Result;


            // assert
            Assert.IsTrue(result1.Success);
            Assert.IsFalse(result2.Success);
        }

        [TestCase]
        public void SettingRateLimitingBehaviourToWait_Should_DelayLimitedRequests()
        {
            // arrange
            var client = new TestRestClient(new RestClientOptions("")
            {
                RateLimiters = new List<IRateLimiter> { new RateLimiterTotal(1, TimeSpan.FromSeconds(1)) },
                RateLimitingBehaviour = RateLimitingBehaviour.Wait
            });
            client.SetResponse("{\"property\": 123}");


            // act
            var sw = Stopwatch.StartNew();
            var result1 = client.Request<TestObject>().Result;
            client.SetResponse("{\"property\": 123}"); // reset response stream
            var result2 = client.Request<TestObject>().Result;
            sw.Stop();

            // assert
            Assert.IsTrue(result1.Success);
            Assert.IsTrue(result2.Success);
            Assert.IsTrue(sw.ElapsedMilliseconds > 900, $"Actual: {sw.ElapsedMilliseconds}");
        }

        [TestCase]
        public void SettingApiKeyRateLimiter_Should_DelayRequestsFromSameKey()
        {
            // arrange
            var client = new TestRestClient(new RestClientOptions("")
            {
                RateLimiters = new List<IRateLimiter> { new RateLimiterAPIKey(1, TimeSpan.FromSeconds(1)) },
                RateLimitingBehaviour = RateLimitingBehaviour.Wait,
                LogLevel = LogLevel.Debug,
                ApiCredentials = new ApiCredentials("TestKey", "TestSecret")
            });
            client.SetResponse("{\"property\": 123}");


            // act
            var sw = Stopwatch.StartNew();
            var result1 = client.Request<TestObject>().Result;
            client.SetKey("TestKey2", "TestSecret2"); // set to different key
            client.SetResponse("{\"property\": 123}"); // reset response stream
            var result2 = client.Request<TestObject>().Result;
            client.SetKey("TestKey", "TestSecret"); // set back to original key, should delay
            client.SetResponse("{\"property\": 123}"); // reset response stream
            var result3 = client.Request<TestObject>().Result;
            sw.Stop();

            // assert
            Assert.IsTrue(result1.Success);
            Assert.IsTrue(result2.Success);
            Assert.IsTrue(result3.Success);
            Assert.IsTrue(sw.ElapsedMilliseconds > 900 && sw.ElapsedMilliseconds < 1900, $"Actual: {sw.ElapsedMilliseconds}");
        }
    }
}
