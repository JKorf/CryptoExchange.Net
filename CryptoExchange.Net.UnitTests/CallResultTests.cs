using CryptoExchange.Net.Objects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    internal class CallResultTests
    {
        [Test]
        public void TestBasicErrorCallResult()
        {
            var result = new CallResult(new ServerError("TestError"));

            Assert.AreEqual(result.Error.Message, "TestError");
            Assert.IsFalse(result);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TestBasicSuccessCallResult()
        {
            var result = new CallResult(null);

            Assert.IsNull(result.Error);
            Assert.IsTrue(result);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void TestCallResultError()
        {
            var result = new CallResult<object>(new ServerError("TestError"));

            Assert.AreEqual(result.Error.Message, "TestError");
            Assert.IsNull(result.Data);
            Assert.IsFalse(result);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void TestCallResultSuccess()
        {
            var result = new CallResult<object>(new object());

            Assert.IsNull(result.Error);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void TestCallResultSuccessAs()
        {
            var result = new CallResult<TestObjectResult>(new TestObjectResult());
            var asResult = result.As<TestObject2>(result.Data.InnerData);

            Assert.IsNull(asResult.Error);
            Assert.IsNotNull(asResult.Data);
            Assert.IsTrue(asResult.Data is TestObject2);
            Assert.IsTrue(asResult);
            Assert.IsTrue(asResult.Success);
        }

        [Test]
        public void TestCallResultErrorAs()
        {
            var result = new CallResult<TestObjectResult>(new ServerError("TestError"));
            var asResult = result.As<TestObject2>(default);

            Assert.IsNotNull(asResult.Error);
            Assert.AreEqual(asResult.Error.Message, "TestError");
            Assert.IsNull(asResult.Data);
            Assert.IsFalse(asResult);
            Assert.IsFalse(asResult.Success);
        }

        [Test]
        public void TestCallResultErrorAsError()
        {
            var result = new CallResult<TestObjectResult>(new ServerError("TestError"));
            var asResult = result.AsError<TestObject2>(new ServerError("TestError2"));

            Assert.IsNotNull(asResult.Error);
            Assert.AreEqual(asResult.Error.Message, "TestError2");
            Assert.IsNull(asResult.Data);
            Assert.IsFalse(asResult);
            Assert.IsFalse(asResult.Success);
        }

        [Test]
        public void TestWebCallResultErrorAsError()
        {
            var result = new WebCallResult<TestObjectResult>(new ServerError("TestError"));
            var asResult = result.AsError<TestObject2>(new ServerError("TestError2"));

            Assert.IsNotNull(asResult.Error);
            Assert.AreEqual(asResult.Error.Message, "TestError2");
            Assert.IsNull(asResult.Data);
            Assert.IsFalse(asResult);
            Assert.IsFalse(asResult.Success);
        }

        [Test]
        public void TestWebCallResultSuccessAsError()
        {
            var result = new WebCallResult<TestObjectResult>(
                System.Net.HttpStatusCode.OK,
                new List<KeyValuePair<string, IEnumerable<string>>>(),
                TimeSpan.FromSeconds(1),
                "{}",
                "https://test.com/api",
                null,
                HttpMethod.Get,
                new List<KeyValuePair<string, IEnumerable<string>>>(),
                new TestObjectResult(),
                null);
            var asResult = result.AsError<TestObject2>(new ServerError("TestError2"));

            Assert.IsNotNull(asResult.Error);
            Assert.AreEqual(asResult.Error.Message, "TestError2");
            Assert.AreEqual(asResult.ResponseStatusCode, System.Net.HttpStatusCode.OK);
            Assert.AreEqual(asResult.ResponseTime, TimeSpan.FromSeconds(1));
            Assert.AreEqual(asResult.RequestUrl, "https://test.com/api");
            Assert.AreEqual(asResult.RequestMethod, HttpMethod.Get);
            Assert.IsNull(asResult.Data);
            Assert.IsFalse(asResult);
            Assert.IsFalse(asResult.Success);
        }

        [Test]
        public void TestWebCallResultSuccessAsSuccess()
        {
            var result = new WebCallResult<TestObjectResult>(
                System.Net.HttpStatusCode.OK,
                new List<KeyValuePair<string, IEnumerable<string>>>(),
                TimeSpan.FromSeconds(1),
                "{}",
                "https://test.com/api",
                null,
                HttpMethod.Get,
                new List<KeyValuePair<string, IEnumerable<string>>>(),
                new TestObjectResult(),
                null);
            var asResult = result.As<TestObject2>(result.Data.InnerData);

            Assert.IsNull(asResult.Error);
            Assert.AreEqual(asResult.ResponseStatusCode, System.Net.HttpStatusCode.OK);
            Assert.AreEqual(asResult.ResponseTime, TimeSpan.FromSeconds(1));
            Assert.AreEqual(asResult.RequestUrl, "https://test.com/api");
            Assert.AreEqual(asResult.RequestMethod, HttpMethod.Get);
            Assert.IsNotNull(asResult.Data);
            Assert.IsTrue(asResult);
            Assert.IsTrue(asResult.Success);
        }
    }

    public class TestObjectResult
    {
        public TestObject2 InnerData;

        public TestObjectResult()
        {
            InnerData = new TestObject2();
        }
    }

    public class TestObject2
    {
    }
}
