using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
    }
}
