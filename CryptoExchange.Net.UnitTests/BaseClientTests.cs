using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class BaseClientTests
    {
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("test", null)]
        [TestCase("test", "")]
        [TestCase(null, "test")]
        [TestCase("", "test")]
        public void SettingEmptyValuesForAPICredentials_Should_ThrowException(string key, string secret)
        {
            // arrange
            // act
            // assert
            Assert.Throws(typeof(ArgumentException), () => new TestBaseClient(new RestClientOptions("") { ApiCredentials = new ApiCredentials(key, secret) }));
        }

        [TestCase]
        public void SettingLogOutput_Should_RedirectLogOutput()
        {
            // arrange
            var logger = new TestStringLogger();
            var client = new TestBaseClient(new RestClientOptions("")
            {
                LogWriters = new List<ILogger> { logger }
            });

            // act
            client.Log(LogLevel.Information, "Test");

            // assert
            Assert.IsFalse(string.IsNullOrEmpty(logger.GetLogs()));
        }

        [TestCase(LogLevel.None, LogLevel.Error, false)]
        [TestCase(LogLevel.None, LogLevel.Warning, false)]
        [TestCase(LogLevel.None, LogLevel.Information, false)]
        [TestCase(LogLevel.None, LogLevel.Debug, false)]
        [TestCase(LogLevel.Error, LogLevel.Error, true)]
        [TestCase(LogLevel.Error, LogLevel.Warning, false)]
        [TestCase(LogLevel.Error, LogLevel.Information, false)]
        [TestCase(LogLevel.Error, LogLevel.Debug, false)]
        [TestCase(LogLevel.Warning, LogLevel.Error, true)]
        [TestCase(LogLevel.Warning, LogLevel.Warning, true)]
        [TestCase(LogLevel.Warning, LogLevel.Information, false)]
        [TestCase(LogLevel.Warning, LogLevel.Debug, false)]
        [TestCase(LogLevel.Information, LogLevel.Error, true)]
        [TestCase(LogLevel.Information, LogLevel.Warning, true)]
        [TestCase(LogLevel.Information, LogLevel.Information, true)]
        [TestCase(LogLevel.Information, LogLevel.Debug, false)]
        [TestCase(LogLevel.Debug, LogLevel.Error, true)]
        [TestCase(LogLevel.Debug, LogLevel.Warning, true)]
        [TestCase(LogLevel.Debug, LogLevel.Information, true)]
        [TestCase(LogLevel.Debug, LogLevel.Debug, true)]
        [TestCase(null, LogLevel.Error, true)]
        [TestCase(null, LogLevel.Warning, true)]
        [TestCase(null, LogLevel.Information, true)]
        [TestCase(null, LogLevel.Debug, true)]
        public void SettingLogLevel_Should_RestrictLogging(LogLevel? verbosity, LogLevel testVerbosity, bool expected)
        {
            // arrange
            var logger = new TestStringLogger();
            var client = new TestBaseClient(new RestClientOptions("")
            {
                LogWriters = new List<ILogger> { logger },
                LogLevel = verbosity
            });

            // act
            client.Log(testVerbosity, "Test");

            // assert
            Assert.AreEqual(!string.IsNullOrEmpty(logger.GetLogs()), expected);
        }

        [TestCase]
        public void DeserializingValidJson_Should_GiveSuccessfulResult()
        {
            // arrange
            var client = new TestBaseClient();

            // act
            var result = client.Deserialize<object>("{\"testProperty\": 123}");

            // assert
            Assert.IsTrue(result.Success);
        }

        [TestCase]
        public void DeserializingInvalidJson_Should_GiveErrorResult()
        {
            // arrange
            var client = new TestBaseClient();

            // act
            var result = client.Deserialize<object>("{\"testProperty\": 123");

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
        }

        [TestCase]
        public void FillingPathParameters_Should_ResultInValidUrl()
        {
            // arrange
            var client = new TestBaseClient();

            // act
            var result = client.FillParameters("http://test.api/{}/path/{}", "1", "test");

            // assert
            Assert.IsTrue(result == "http://test.api/1/path/test");
        }
    }
}
