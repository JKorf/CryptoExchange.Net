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
using System.Net.Http;
using System.Threading.Tasks;

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
            var client = new TestRestClient(new RestClientOptions("")
            {
                BaseAddress = "http://test.address.com",
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

        [TestCase]
        public void SettingRateLimitingBehaviourToFail_Should_FailLimitedRequests()
        {
            // arrange
            var client = new TestRestClient(new RestClientOptions("")
            {
                RateLimiters = new List<IRateLimiter> { new RateLimiterTotal(1, TimeSpan.FromSeconds(1)) },
                RateLimitingBehaviour = RateLimitingBehaviour.Fail
            });
            client.SetResponse("{\"property\": 123}", out _);


            // act
            var result1 = client.Request<TestObject>().Result;
            client.SetResponse("{\"property\": 123}", out _);
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
            client.SetResponse("{\"property\": 123}", out _);


            // act
            var sw = Stopwatch.StartNew();
            var result1 = client.Request<TestObject>().Result;
            client.SetResponse("{\"property\": 123}", out _); // reset response stream
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
            client.SetResponse("{\"property\": 123}", out _);


            // act
            var sw = Stopwatch.StartNew();
            var result1 = client.Request<TestObject>().Result;
            client.SetKey("TestKey2", "TestSecret2"); // set to different key
            client.SetResponse("{\"property\": 123}", out _); // reset response stream
            var result2 = client.Request<TestObject>().Result;
            client.SetKey("TestKey", "TestSecret"); // set back to original key, should delay
            client.SetResponse("{\"property\": 123}", out _); // reset response stream
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
