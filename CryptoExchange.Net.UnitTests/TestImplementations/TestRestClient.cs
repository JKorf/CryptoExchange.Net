using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using System.Collections.Generic;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestRestClient: RestClient
    {
        public TestRestClient() : base("Test", new RestClientOptions("http://testurl.url"), null)
        {
            RequestFactory = new Mock<IRequestFactory>().Object;
        }

        public TestRestClient(RestClientOptions exchangeOptions) : base("Test", exchangeOptions, exchangeOptions.ApiCredentials == null ? null : new TestAuthProvider(exchangeOptions.ApiCredentials))
        {
            RequestFactory = new Mock<IRequestFactory>().Object;
        }

        public void SetParameterPosition(HttpMethod method, HttpMethodParameterPosition position)
        {
            ParameterPositions[method] = position;
        }

        public void SetKey(string key, string secret)
        {
            SetAuthenticationProvider(new UnitTests.TestAuthProvider(new ApiCredentials(key, secret)));
        }

        public void SetResponse(string responseData, out IRequest requestObj)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(responseData);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);

            var response = new Mock<IResponse>();
            response.Setup(c => c.IsSuccessStatusCode).Returns(true);
            response.Setup(c => c.GetResponseStreamAsync()).Returns(Task.FromResult((Stream)responseStream));
            
            var headers = new Dictionary<string, IEnumerable<string>>();
            var request = new Mock<IRequest>();
            request.Setup(c => c.Uri).Returns(new Uri("http://www.test.com"));
            request.Setup(c => c.GetResponseAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));
            request.Setup(c => c.SetContent(It.IsAny<string>(), It.IsAny<string>())).Callback(new Action<string, string>((content, type) => { request.Setup(r => r.Content).Returns(content); }));
            request.Setup(c => c.AddHeader(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((key, val) => headers.Add(key, new List<string> { val }));
            request.Setup(c => c.GetHeaders()).Returns(() => headers);

            var factory = Mock.Get(RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<HttpMethod, string, int>((method, uri, id) => 
                { 
                    request.Setup(a => a.Uri).Returns(new Uri(uri));
                    request.Setup(a => a.Method).Returns(method); 
                })
                .Returns(request.Object);
            requestObj = request.Object;
        }

        public void SetErrorWithoutResponse(HttpStatusCode code, string message)
        {
            var we = new HttpRequestException();
            typeof(HttpRequestException).GetField("_message", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(we, message);
           
            var request = new Mock<IRequest>();
            request.Setup(c => c.GetResponseAsync(It.IsAny<CancellationToken>())).Throws(we);

            var factory = Mock.Get(RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(request.Object);
        }

        public void SetErrorWithResponse(string responseData, HttpStatusCode code)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(responseData);
            var responseStream = new MemoryStream();
            responseStream.Write(expectedBytes, 0, expectedBytes.Length);
            responseStream.Seek(0, SeekOrigin.Begin);

            var response = new Mock<IResponse>();
            response.Setup(c => c.IsSuccessStatusCode).Returns(false);
            response.Setup(c => c.GetResponseStreamAsync()).Returns(Task.FromResult((Stream)responseStream));

            var headers = new Dictionary<string, IEnumerable<string>>();
            var request = new Mock<IRequest>();
            request.Setup(c => c.Uri).Returns(new Uri("http://www.test.com"));
            request.Setup(c => c.GetResponseAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));
            request.Setup(c => c.AddHeader(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((key, val) => headers.Add(key, new List<string> { val }));
            request.Setup(c => c.GetHeaders()).Returns(headers);

            var factory = Mock.Get(RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<HttpMethod, string, int>((method, uri, id) => request.Setup(a => a.Uri).Returns(new Uri(uri)))
                .Returns(request.Object);
        }

        public async Task<CallResult<T>> Request<T>(CancellationToken ct = default) where T:class
        {
            return await SendRequestAsync<T>(new Uri("http://www.test.com"), HttpMethod.Get, ct);
        }

        public async Task<CallResult<T>> RequestWithParams<T>(HttpMethod method, Dictionary<string, object> parameters, Dictionary<string, string> headers) where T : class
        {
            return await SendRequestAsync<T>(new Uri("http://www.test.com"), method, default, parameters, additionalHeaders: headers);
        }
    }

    public class TestAuthProvider : AuthenticationProvider
    {
        public TestAuthProvider(ApiCredentials credentials) : base(credentials)
        {
        }
    }

    public class ParseErrorTestRestClient: TestRestClient
    {
        public ParseErrorTestRestClient() { }
        public ParseErrorTestRestClient(RestClientOptions exchangeOptions) : base(exchangeOptions) { }

        protected override Error ParseErrorResponse(JToken error)
        {
            return new ServerError((int)error["errorCode"], (string)error["errorMessage"]);
        }
    }
}
