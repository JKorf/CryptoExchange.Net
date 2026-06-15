using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Net;
using System.Net.Http;

namespace CryptoExchange.Net.UnitTests
{
    [TestFixture()]
    internal class CallResultTests
    {
        [Test]
        public void TestBasicErrorCallResult()
        {
            var result = CallResult.Fail(new ServerError("TestError", ErrorInfo.Unknown));

            ClassicAssert.AreSame(result.Error!.ErrorCode, "TestError");
            ClassicAssert.IsFalse(result.Success);
        }

        [Test]
        public void TestBasicSuccessCallResult()
        {
            var result = CallResult.Ok();

            ClassicAssert.IsNull(result.Error);
            Assert.That(result.Success);
        }

        [Test]
        public void TestCallResultError()
        {
            var result = CallResult.Fail<object>(new ServerError("TestError", ErrorInfo.Unknown));

            ClassicAssert.AreSame(result.Error!.ErrorCode, "TestError");
            ClassicAssert.IsNull(result.Data);
            ClassicAssert.IsFalse(result.Success);
        }

        [Test]
        public void TestCallResultSuccess()
        {
            var result = CallResult.Ok<object>(new object());

            ClassicAssert.IsNull(result.Error);
            ClassicAssert.IsNotNull(result.Data);
            Assert.That(result.Success);
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
