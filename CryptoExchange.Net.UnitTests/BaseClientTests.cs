using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            var stringBuilder = new StringBuilder();
            var client = new TestBaseClient(new RestClientOptions("")
            {
                LogWriters = new List<TextWriter> { new StringWriter(stringBuilder) }
            });

            // act
            client.Log(LogVerbosity.Info, "Test");

            // assert
            Assert.IsFalse(string.IsNullOrEmpty(stringBuilder.ToString()));
        }

        [TestCase(LogVerbosity.None, LogVerbosity.Error, false)]
        [TestCase(LogVerbosity.None, LogVerbosity.Warning, false)]
        [TestCase(LogVerbosity.None, LogVerbosity.Info, false)]
        [TestCase(LogVerbosity.None, LogVerbosity.Debug, false)]
        [TestCase(LogVerbosity.Error, LogVerbosity.Error, true)]
        [TestCase(LogVerbosity.Error, LogVerbosity.Warning, false)]
        [TestCase(LogVerbosity.Error, LogVerbosity.Info, false)]
        [TestCase(LogVerbosity.Error, LogVerbosity.Debug, false)]
        [TestCase(LogVerbosity.Warning, LogVerbosity.Error, true)]
        [TestCase(LogVerbosity.Warning, LogVerbosity.Warning, true)]
        [TestCase(LogVerbosity.Warning, LogVerbosity.Info, false)]
        [TestCase(LogVerbosity.Warning, LogVerbosity.Debug, false)]
        [TestCase(LogVerbosity.Info, LogVerbosity.Error, true)]
        [TestCase(LogVerbosity.Info, LogVerbosity.Warning, true)]
        [TestCase(LogVerbosity.Info, LogVerbosity.Info, true)]
        [TestCase(LogVerbosity.Info, LogVerbosity.Debug, false)]
        [TestCase(LogVerbosity.Debug, LogVerbosity.Error, true)]
        [TestCase(LogVerbosity.Debug, LogVerbosity.Warning, true)]
        [TestCase(LogVerbosity.Debug, LogVerbosity.Info, true)]
        [TestCase(LogVerbosity.Debug, LogVerbosity.Debug, true)]
        public void SettingLogVerbosity_Should_RestrictLogging(LogVerbosity verbosity, LogVerbosity testVerbosity, bool expected)
        {
            // arrange
            var stringBuilder = new StringBuilder();
            var client = new TestBaseClient(new RestClientOptions("")
            {
                LogWriters = new List<TextWriter> { new StringWriter(stringBuilder) },
                LogVerbosity = verbosity
            });

            // act
            client.Log(testVerbosity, "Test");

            // assert
            Assert.AreEqual(!string.IsNullOrEmpty(stringBuilder.ToString()), expected);
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
