using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class BaseClientTests
    {
        [TestCase]
        public void DeserializingValidJson_Should_GiveSuccessfulResult()
        {
            // arrange
            var client = new TestBaseClient();

            // act
            var result = client.SubClient.Deserialize<object>("{\"testProperty\": 123}");

            // assert
            Assert.That(result.Success);
        }

        [TestCase]
        public void DeserializingInvalidJson_Should_GiveErrorResult()
        {
            // arrange
            var client = new TestBaseClient();

            // act
            var result = client.SubClient.Deserialize<object>("{\"testProperty\": 123");

            // assert
            ClassicAssert.IsFalse(result.Success);
            Assert.That(result.Error != null);
        }

        [TestCase("https://api.test.com/api", new[] { "path1", "path2" }, "https://api.test.com/api/path1/path2")]
        [TestCase("https://api.test.com/api", new[] { "path1", "/path2" }, "https://api.test.com/api/path1/path2")]
        [TestCase("https://api.test.com/api", new[] { "path1/", "path2" }, "https://api.test.com/api/path1/path2")]
        [TestCase("https://api.test.com/api", new[] { "path1/", "/path2" }, "https://api.test.com/api/path1/path2")]
        [TestCase("https://api.test.com/api/", new[] { "path1", "path2" }, "https://api.test.com/api/path1/path2")]
        [TestCase("https://api.test.com", new[] { "test-path/test-path" }, "https://api.test.com/test-path/test-path")]
        [TestCase("https://api.test.com/", new[] { "test-path/test-path" }, "https://api.test.com/test-path/test-path")]
        public void AppendPathTests(string baseUrl, string[] path, string expected)
        {
            var result = baseUrl.AppendPath(path);
            Assert.That(expected == result);
        }
    }
}
