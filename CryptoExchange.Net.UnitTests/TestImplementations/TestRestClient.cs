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
using CryptoExchange.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.SharedApis;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestRestClient: BaseRestClient
    {
        public TestRestApi1Client Api1 { get; }
        public TestRestApi2Client Api2 { get; }

        public TestRestClient(Action<TestClientOptions> optionsFunc) : this(optionsFunc, null)
        {
        }

        public TestRestClient(ILoggerFactory loggerFactory = null, HttpClient httpClient = null) : this((x) => { }, httpClient, loggerFactory)
        {
        }

        public TestRestClient(Action<TestClientOptions> optionsFunc, HttpClient httpClient = null, ILoggerFactory loggerFactory = null) : base(loggerFactory, "Test")
        {
            var options = TestClientOptions.Default.Copy();
            optionsFunc(options);
            Initialize(options);

            Api1 = new TestRestApi1Client(options);
            Api2 = new TestRestApi2Client(options);
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

            var factory = Mock.Get(Api1.RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<int>()))
                .Callback<HttpMethod, Uri, int>((method, uri, id) => 
                { 
                    request.Setup(a => a.Uri).Returns(uri);
                    request.Setup(a => a.Method).Returns(method); 
                })
                .Returns(request.Object);

            factory = Mock.Get(Api2.RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<int>()))
                .Callback<HttpMethod, Uri, int>((method, uri, id) =>
                {
                    request.Setup(a => a.Uri).Returns(uri);
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
            request.Setup(c => c.Uri).Returns(new Uri("http://www.test.com"));
            request.Setup(c => c.GetHeaders()).Returns(new Dictionary<string, IEnumerable<string>>());
            request.Setup(c => c.GetResponseAsync(It.IsAny<CancellationToken>())).Throws(we);

            var factory = Mock.Get(Api1.RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<int>()))
                .Returns(request.Object);


            factory = Mock.Get(Api2.RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<int>()))
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

            var factory = Mock.Get(Api1.RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<int>()))
                .Callback<HttpMethod, Uri, int>((method, uri, id) => request.Setup(a => a.Uri).Returns(uri))
                .Returns(request.Object);

            factory = Mock.Get(Api2.RequestFactory);
            factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<int>()))
                .Callback<HttpMethod, Uri, int>((method, uri, id) => request.Setup(a => a.Uri).Returns(uri))
                .Returns(request.Object);
        }
    }

    public class TestRestApi1Client : RestApiClient
    {
        public TestRestApi1Client(TestClientOptions options) : base(new TraceLogger(), null, "https://localhost:123", options, options.Api1Options)
        {
            RequestFactory = new Mock<IRequestFactory>().Object;
        }

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode futuresType, DateTime? deliverDate = null) => $"{baseAsset.ToUpperInvariant()}{quoteAsset.ToUpperInvariant()}";

        public async Task<CallResult<T>> Request<T>(CancellationToken ct = default) where T : class
        {
            return await SendRequestAsync<T>(new Uri("http://www.test.com"), HttpMethod.Get, ct, requestWeight: 0);
        }

        public async Task<CallResult<T>> RequestWithParams<T>(HttpMethod method, Dictionary<string, object> parameters, Dictionary<string, string> headers) where T : class
        {
            return await SendRequestAsync<T>(new Uri("http://www.test.com"), method, default, parameters, requestWeight: 0, additionalHeaders: headers);
        }

        public void SetParameterPosition(HttpMethod method, HttpMethodParameterPosition position)
        {
            ParameterPositions[method] = position;
        }

        public override TimeSpan? GetTimeOffset()
        {
            throw new NotImplementedException();
        }

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new TestAuthProvider(credentials);

        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync()
        {
            throw new NotImplementedException();
        }

        public override TimeSyncInfo GetTimeSyncInfo()
        {
            throw new NotImplementedException();
        }
    }

    public class TestRestApi2Client : RestApiClient
    {
        public TestRestApi2Client(TestClientOptions options) : base(new TraceLogger(), null, "https://localhost:123", options, options.Api2Options)
        {
            RequestFactory = new Mock<IRequestFactory>().Object;
        }

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode futuresType, DateTime? deliverDate = null) => $"{baseAsset.ToUpperInvariant()}{quoteAsset.ToUpperInvariant()}";

        public async Task<CallResult<T>> Request<T>(CancellationToken ct = default) where T : class
        {
            return await SendRequestAsync<T>(new Uri("http://www.test.com"), HttpMethod.Get, ct, requestWeight: 0);
        }

        protected override Error ParseErrorResponse(int httpStatusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders, IMessageAccessor accessor)
        {
            var errorData = accessor.Deserialize<TestError>();

            return new ServerError(errorData.Data.ErrorCode, errorData.Data.ErrorMessage);
        }

        public override TimeSpan? GetTimeOffset()
        {
            throw new NotImplementedException();
        }

        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new TestAuthProvider(credentials);

        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync()
        {
            throw new NotImplementedException();
        }

        public override TimeSyncInfo GetTimeSyncInfo()
        {
            throw new NotImplementedException();
        }
    }

    public class TestError
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ParseErrorTestRestClient: TestRestClient
    {
        public ParseErrorTestRestClient() { }

    }
}
