using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.RateLimiter;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    public class ExchangeClientTests
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
            var client = PrepareClient("");

            // act
            // assert
            Assert.Throws(typeof(ArgumentException), () => client.SetApiCredentails(key, secret));
        }

        [TestCase()]
        public void SettingLogOutput_Should_RedirectLogOutput()
        {
            // arrange
            var stringBuilder = new StringBuilder();
            var client = PrepareClient("{}", true, LogVerbosity.Debug, new StringWriter(stringBuilder));

            // act
            client.TestCall();

            // assert
            Assert.IsFalse(string.IsNullOrEmpty(stringBuilder.ToString()));
        }

        [TestCase()]
        public void ObjectDeserializationFail_Should_GiveFailedResult()
        {
            // arrange
            var errorMessage = "TestErrorMessage";
            var client = PrepareClient(JsonConvert.SerializeObject(errorMessage));

            // act
            var result = client.TestCall();

            // assert
            Assert.IsFalse(result.Success);
            Assert.AreNotEqual(0, result.Error.Code);
            Assert.IsTrue(result.Error.Message.Contains(errorMessage));
        }

        [TestCase()]
        public void InvalidJson_Should_GiveFailedResult()
        {
            // arrange
            var errorMessage = "TestErrorMessage";
            var client = PrepareClient(JsonConvert.SerializeObject(errorMessage));

            // act
            var result = client.TestCall();

            // assert
            Assert.IsFalse(result.Success);
            Assert.AreNotEqual(0, result.Error.Code);
            Assert.IsTrue(result.Error.Message.Contains(errorMessage));
        }

        [TestCase()]
        public void WhenUsingRateLimiterTotalRequests_Should_BeDelayed()
        {
            // arrange
            var client = PrepareClient(JsonConvert.SerializeObject(new TestObject()));
            client.AddRateLimiter(new RateLimiterTotal(1, TimeSpan.FromSeconds(5)));

            // act
            var sw = Stopwatch.StartNew();
            client.TestCall();
            client.TestCall();
            client.TestCall();
            sw.Stop();

            // assert
            Assert.IsTrue(sw.ElapsedMilliseconds > 9000);
        }

        [TestCase()]
        public void WhenUsingRateLimiterPerEndpointRequests_Should_BeDelayed()
        {
            // arrange
            var client = PrepareClient(JsonConvert.SerializeObject(new TestObject()));
            client.AddRateLimiter(new RateLimiterTotal(1, TimeSpan.FromSeconds(5)));

            // act
            var sw = Stopwatch.StartNew();
            client.TestCall();
            client.TestCall();
            client.TestCall();
            sw.Stop();

            // assert
            Assert.IsTrue(sw.ElapsedMilliseconds > 9000);
        }

        [TestCase()]
        public void WhenRemovingRateLimiterRequest_Should_NoLongerBeDelayed()
        {
            // arrange
            var client = PrepareClient(JsonConvert.SerializeObject(new TestObject()));
            client.AddRateLimiter(new RateLimiterTotal(1, TimeSpan.FromSeconds(5)));
            client.RemoveRateLimiters();

            // act
            var sw = Stopwatch.StartNew();
            client.TestCall();
            client.TestCall();
            client.TestCall();
            sw.Stop();

            // assert
            Assert.IsTrue(sw.ElapsedMilliseconds < 5000);
        }

        [TestCase()]
        public void ReceivingErrorStatusCode_Should_NotSuccess()
        {
            // arrange
            var client = PrepareExceptionClient(JsonConvert.SerializeObject(new TestObject()), "InvalidStatusCodeResponse", 203);

            // act
            var result = client.TestCall();

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error.Message.Contains("InvalidStatusCodeResponse"));
        }

        private TestImplementation PrepareClient(string responseData, bool withOptions = true, LogVerbosity verbosity = LogVerbosity.Warning, TextWriter tw = null)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(responseData);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);

            var response = new Mock<IResponse>();
            response.Setup(c => c.GetResponseStream()).Returns(responseStream);

            var request = new Mock<IRequest>();
            request.Setup(c => c.Headers).Returns(new WebHeaderCollection());
            request.Setup(c => c.Uri).Returns(new Uri("http://www.test.com"));
            request.Setup(c => c.GetResponse()).Returns(Task.FromResult(response.Object));

            var factory = new Mock<IRequestFactory>();
            factory.Setup(c => c.Create(It.IsAny<string>()))
                .Returns(request.Object);
            TestImplementation client;
            if (withOptions)
            {
                var options = new ExchangeOptions()
                {
                    ApiCredentials = new ApiCredentials("Test", "Test2"),
                    LogVerbosity = verbosity
                };
                if (tw != null)
                    options.LogWriters = new List<TextWriter>() { tw };

                client = new TestImplementation(options);
            }
            else
            {
                client = new TestImplementation();
            }
            client.RequestFactory = factory.Object;
            return client;
        }

        private TestImplementation PrepareExceptionClient(string responseData, string exceptionMessage, int statusCode, bool credentials = true)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(responseData);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);

            var we = new WebException();
            var r = new HttpWebResponse();
            var re = new HttpResponseMessage();

            typeof(HttpResponseMessage).GetField("_statusCode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(re, (HttpStatusCode)statusCode);
            typeof(HttpWebResponse).GetField("_httpResponseMessage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(r, re);
            typeof(WebException).GetField("_message", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(we, exceptionMessage);
            typeof(WebException).GetField("_response", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(we, r);

            var response = new Mock<IResponse>();
            response.Setup(c => c.GetResponseStream()).Throws(we);

            var request = new Mock<IRequest>();
            request.Setup(c => c.Headers).Returns(new WebHeaderCollection());
            request.Setup(c => c.GetResponse()).Returns(Task.FromResult(response.Object));

            var factory = new Mock<IRequestFactory>();
            factory.Setup(c => c.Create(It.IsAny<string>()))
                .Returns(request.Object);

            TestImplementation client = credentials ? new TestImplementation(new ExchangeOptions() { ApiCredentials = new ApiCredentials("Test", "Test2") }) : new TestImplementation();
            client.RequestFactory = factory.Object;
            return client;
        }
    }
}
