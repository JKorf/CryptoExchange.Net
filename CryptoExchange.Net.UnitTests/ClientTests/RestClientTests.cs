using CryptoExchange.Net.Objects;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using NUnit.Framework.Legacy;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Interfaces;
using System.Text.Json;
using CryptoExchange.Net.UnitTests.Implementations;
using CryptoExchange.Net.Testing;

namespace CryptoExchange.Net.UnitTests.ClientTests
{
    [TestFixture()]
    public class RestClientTests
    {
        [TestCase]
        public async Task RequestingData_Should_ResultInData()
        {
            // arrange
            var client = new TestRestClient();
            var expected = new TestObject() { DecimalData = 1.23M, IntData = 10, StringData = "Some data" };
            var strData = JsonSerializer.Serialize(expected, new JsonSerializerOptions { TypeInfoResolver = new TestSerializerContext() });
            client.ApiClient1.SetNextResponse(strData, System.Net.HttpStatusCode.OK);

            // act
            var result = await client.ApiClient1.GetResponseAsync<TestObject>();

            // assert
            Assert.That(result.Success);
            Assert.That(TestHelpers.AreEqual(expected, result.Data));
        }

        [TestCase]
        public async Task ReceivingInvalidData_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.ApiClient1.SetNextResponse("{\"property\": 123", System.Net.HttpStatusCode.OK);

            // act
            var result = await client.ApiClient1.GetResponseAsync<TestObject>();

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
        }

        [TestCase]
        public async Task ReceivingErrorCode_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.ApiClient1.SetNextResponse("Invalid request", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = await client.ApiClient1.GetResponseAsync<TestObject>();

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
        }

        [TestCase]
        public async Task ReceivingErrorAndNotParsingError_Should_ResultInFlatError()
        {
            // arrange
            var client = new TestRestClient();
            client.ApiClient1.SetNextResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = await client.ApiClient1.GetResponseAsync<TestObject>();

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
            Assert.That(result.Error is ServerError);
        }

        [TestCase]
        public async Task ReceivingErrorAndNotParsingErrorAndInvalidJson_Should_ContainData()
        {
            // arrange
            var client = new TestRestClient();
            var response = "<html>...</html>";
            client.ApiClient1.SetNextResponse(response, System.Net.HttpStatusCode.BadRequest);

            // act
            var result = await client.ApiClient1.GetResponseAsync<TestObject>();

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
            Assert.That(result.Error is DeserializeError);
            Assert.That(result.Error!.Message!.Contains(response));
        }

        [TestCase]
        public async Task ReceivingErrorAndParsingError_Should_ResultInParsedError()
        {
            // arrange
            var client = new TestRestClient();
            client.ApiClient1.SetNextResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = await client.ApiClient1.GetResponseAsync<TestObject>();

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
            Assert.That(result.Error is ServerError);
            Assert.That(result.Error!.ErrorCode == "123");
            Assert.That(result.Error.Message == "Invalid request");
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

            var httpMethod = new HttpMethod(method);
            client.ApiClient1.SetParameterPosition(httpMethod, pos);
            client.ApiClient1.SetNextResponse("{}", System.Net.HttpStatusCode.OK);

            var result = await client.ApiClient1.GetResponseAsync<TestObject>(httpMethod, new ParameterCollection
            {
                { "TestParam1", "Value1" },
                { "TestParam2", 2 },
            });

            // assert
            Assert.That(result.RequestMethod == new HttpMethod(method));
            Assert.That(result.RequestBody?.Contains("TestParam1") == true == (pos == HttpMethodParameterPosition.InBody));
            Assert.That((result.RequestUrl?.ToString().Contains("TestParam1")) == (pos == HttpMethodParameterPosition.InUri));
            Assert.That(result.RequestBody?.Contains("TestParam2") == true == (pos == HttpMethodParameterPosition.InBody));
            Assert.That((result.RequestUrl?.ToString().Contains("TestParam2")) == (pos == HttpMethodParameterPosition.InUri));
        }
    }
}
