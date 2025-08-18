using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            var result = new CallResult(new ServerError("TestError", ErrorInfo.Unknown));

            ClassicAssert.AreSame(result.Error.ErrorCode, "TestError");
            ClassicAssert.IsFalse(result);
            ClassicAssert.IsFalse(result.Success);
        }

        [Test]
        public void TestBasicSuccessCallResult()
        {
            var result = new CallResult(null);

            ClassicAssert.IsNull(result.Error);
            Assert.That(result);
            Assert.That(result.Success);
        }

        [Test]
        public void TestCallResultError()
        {
            var result = new CallResult<object>(new ServerError("TestError", ErrorInfo.Unknown));

            ClassicAssert.AreSame(result.Error.ErrorCode, "TestError");
            ClassicAssert.IsNull(result.Data);
            ClassicAssert.IsFalse(result);
            ClassicAssert.IsFalse(result.Success);
        }

        [Test]
        public void TestCallResultSuccess()
        {
            var result = new CallResult<object>(new object());

            ClassicAssert.IsNull(result.Error);
            ClassicAssert.IsNotNull(result.Data);
            Assert.That(result);
            Assert.That(result.Success);
        }

        [Test]
        public void TestCallResultSuccessAs()
        {
            var result = new CallResult<TestObjectResult>(new TestObjectResult());
            var asResult = result.As<TestObject2>(result.Data.InnerData);

            ClassicAssert.IsNull(asResult.Error);
            ClassicAssert.IsNotNull(asResult.Data);
            Assert.That(asResult.Data is not null);
            Assert.That(asResult);
            Assert.That(asResult.Success);
        }

        [Test]
        public void TestCallResultErrorAs()
        {
            var result = new CallResult<TestObjectResult>(new ServerError("TestError", ErrorInfo.Unknown));
            var asResult = result.As<TestObject2>(default);

            ClassicAssert.IsNotNull(asResult.Error);
            ClassicAssert.AreSame(asResult.Error.ErrorCode, "TestError");
            ClassicAssert.IsNull(asResult.Data);
            ClassicAssert.IsFalse(asResult);
            ClassicAssert.IsFalse(asResult.Success);
        }

        [Test]
        public void TestCallResultErrorAsError()
        {
            var result = new CallResult<TestObjectResult>(new ServerError("TestError", ErrorInfo.Unknown));
            var asResult = result.AsError<TestObject2>(new ServerError("TestError2", ErrorInfo.Unknown));

            ClassicAssert.IsNotNull(asResult.Error);
            ClassicAssert.AreSame(asResult.Error.ErrorCode, "TestError2");
            ClassicAssert.IsNull(asResult.Data);
            ClassicAssert.IsFalse(asResult);
            ClassicAssert.IsFalse(asResult.Success);
        }

        [Test]
        public void TestWebCallResultErrorAsError()
        {
            var result = new WebCallResult<TestObjectResult>(new ServerError("TestError", ErrorInfo.Unknown));
            var asResult = result.AsError<TestObject2>(new ServerError("TestError2", ErrorInfo.Unknown));

            ClassicAssert.IsNotNull(asResult.Error);
            ClassicAssert.AreSame(asResult.Error.ErrorCode, "TestError2");
            ClassicAssert.IsNull(asResult.Data);
            ClassicAssert.IsFalse(asResult);
            ClassicAssert.IsFalse(asResult.Success);
        }

        [Test]
        public void TestWebCallResultSuccessAsError()
        {
            var result = new WebCallResult<TestObjectResult>(
                System.Net.HttpStatusCode.OK,
                new KeyValuePair<string, string[]>[0],
                TimeSpan.FromSeconds(1),
                null,
                "{}",
                1,
                "https://test.com/api",
                null,
                HttpMethod.Get,
                new KeyValuePair<string, string[]>[0],
                ResultDataSource.Server,
                new TestObjectResult(),
                null);
            var asResult = result.AsError<TestObject2>(new ServerError("TestError2", ErrorInfo.Unknown));

            ClassicAssert.IsNotNull(asResult.Error);
            Assert.That(asResult.Error.ErrorCode == "TestError2");
            Assert.That(asResult.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.That(asResult.ResponseTime == TimeSpan.FromSeconds(1));
            Assert.That(asResult.RequestUrl == "https://test.com/api");
            Assert.That(asResult.RequestMethod == HttpMethod.Get);
            ClassicAssert.IsNull(asResult.Data);
            ClassicAssert.IsFalse(asResult);
            ClassicAssert.IsFalse(asResult.Success);
        }

        [Test]
        public void TestWebCallResultSuccessAsSuccess()
        {
            var result = new WebCallResult<TestObjectResult>(
                System.Net.HttpStatusCode.OK,
                new KeyValuePair<string, string[]>[0],
                TimeSpan.FromSeconds(1),
                null,
                "{}",
                1,
                "https://test.com/api",
                null,
                HttpMethod.Get,
                new KeyValuePair<string, string[]>[0],
                ResultDataSource.Server,
                new TestObjectResult(),
                null);
            var asResult = result.As<TestObject2>(result.Data.InnerData);

            ClassicAssert.IsNull(asResult.Error);
            Assert.That(asResult.ResponseStatusCode == System.Net.HttpStatusCode.OK);
            Assert.That(asResult.ResponseTime == TimeSpan.FromSeconds(1));
            Assert.That(asResult.RequestUrl == "https://test.com/api");
            Assert.That(asResult.RequestMethod == HttpMethod.Get);
            ClassicAssert.IsNotNull(asResult.Data);
            Assert.That(asResult);
            Assert.That(asResult.Success);
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
