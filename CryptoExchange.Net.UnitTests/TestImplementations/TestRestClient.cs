using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestRestClient: RestClient
    {
        public TestRestClient() : base(new ClientOptions(), null)
        {
            RequestFactory = new Mock<IRequestFactory>().Object;
        }

        public TestRestClient(ClientOptions exchangeOptions) : base(exchangeOptions, exchangeOptions.ApiCredentials == null ? null : new TestAuthProvider(exchangeOptions.ApiCredentials))
        {
        }

        public void SetResponse(string responseData)
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

            var factory = Mock.Get(RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<string>()))
                .Returns(request.Object);
        }

        public void SetErrorWithoutResponse(HttpStatusCode code, string message)
        {
            var we = new WebException();
            var r = new HttpWebResponse();
            var re = new HttpResponseMessage();

            typeof(HttpResponseMessage).GetField("_statusCode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(re, code);
            typeof(HttpWebResponse).GetField("_httpResponseMessage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(r, re);
            typeof(WebException).GetField("_message", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(we, message);
            typeof(WebException).GetField("_response", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(we, r);
            
            var request = new Mock<IRequest>();
            request.Setup(c => c.Headers).Returns(new WebHeaderCollection());
            request.Setup(c => c.GetResponse()).Throws(we);

            var factory = Mock.Get(RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<string>()))
                .Returns(request.Object);
        }

        public void SetErrorWithResponse(string responseData, HttpStatusCode code)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(responseData);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);

            var r = new Mock<HttpWebResponse>();
            r.Setup(x => x.GetResponseStream()).Returns(responseStream);
            var we = new WebException("", null, WebExceptionStatus.Success, r.Object);
            
            var request = new Mock<IRequest>();
            request.Setup(c => c.Headers).Returns(new WebHeaderCollection());
            request.Setup(c => c.GetResponse()).Throws(we);

            var factory = Mock.Get(RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<string>()))
                .Returns(request.Object);
        }

        public async Task<CallResult<T>> Request<T>() where T:class
        {
            return await ExecuteRequest<T>(new Uri("http://www.test.com"));
        }
    }

    public class ParseErrorTestRestClient: TestRestClient
    {
        public ParseErrorTestRestClient() { }
        public ParseErrorTestRestClient(ClientOptions exchangeOptions) : base(exchangeOptions) { }

        protected override Error ParseErrorResponse(string error)
        {
            var data = JToken.Parse(error);
            return new ServerError((int)data["errorCode"], (string)data["errorMessage"]);
        }
    }
}
