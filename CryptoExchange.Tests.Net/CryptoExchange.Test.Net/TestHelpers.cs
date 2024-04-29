using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace CryptoExchange.Test.Net
{
    public class TestHelpers
    {
        [ExcludeFromCodeCoverage]
        public static bool AreEqual<T>(T? self, T? to, params string[] ignore) where T : class
        {
            if (self != null && to != null)
            {
                var type = self.GetType();
                var ignoreList = new List<string>(ignore);
                foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (ignoreList.Contains(pi.Name))
                        continue;

                    var selfValue = type.GetProperty(pi.Name)!.GetValue(self, null);
                    var toValue = type.GetProperty(pi.Name)!.GetValue(to, null);

                    if (pi.PropertyType.IsClass && !pi.PropertyType.Module.ScopeName.Equals("System.Private.CoreLib.dll"))
                    {
                        // Check of "CommonLanguageRuntimeLibrary" is needed because string is also a class
                        if (AreEqual(selfValue, toValue, ignore))
                            continue;

                        return false;
                    }

                    if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                        return false;
                }

                return true;
            }

            return self == to;
        }

        public static TestSocket ConfigureSocketClient<T>(T client) where T : BaseSocketClient
        {
            var socket = new TestSocket();
            foreach (var apiClient in client.ApiClients.OfType<SocketApiClient>())
            {
                apiClient.SocketFactory = Mock.Of<IWebsocketFactory>();
                Mock.Get(apiClient.SocketFactory).Setup(f => f.CreateWebsocket(It.IsAny<ILogger>(), It.IsAny<WebSocketParameters>())).Returns(socket);
            }
            return socket;
        }

        public static void ConfigureRestClient<T>(T client, string data, HttpStatusCode code) where T : BaseRestClient
        {
            foreach (var apiClient in client.ApiClients.OfType<RestApiClient>())
            {
                apiClient.RequestFactory = Mock.Of<IRequestFactory>();

                var expectedBytes = Encoding.UTF8.GetBytes(data);
                var responseStream = new MemoryStream();
                responseStream.Write(expectedBytes, 0, expectedBytes.Length);
                responseStream.Seek(0, SeekOrigin.Begin);

                var response = new Mock<IResponse>();
                response.Setup(c => c.StatusCode).Returns(code);
                response.Setup(c => c.IsSuccessStatusCode).Returns(code == HttpStatusCode.OK);
                response.Setup(c => c.GetResponseStreamAsync()).Returns(Task.FromResult((Stream)responseStream));

                var request = new Mock<IRequest>();
                request.Setup(c => c.GetHeaders()).Returns(new Dictionary<string, IEnumerable<string>>());
                request.Setup(c => c.GetResponseAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(response.Object));

                var factory = Mock.Get(apiClient.RequestFactory);
                factory.Setup(c => c.Create(It.IsAny<HttpMethod>(), It.IsAny<Uri>(), It.IsAny<int>()))
                    .Returns((HttpMethod method, Uri uri, int id) =>
                    {
                        request.Setup(c => c.Method).Returns(method);
                        request.Setup(c => c.Uri).Returns(uri);
                        return request.Object;
                    });
            }
        }
    }
}
