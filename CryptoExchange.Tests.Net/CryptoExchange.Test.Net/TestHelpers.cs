﻿using System;
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

        public static T CreateSocketClient<T>(IWebsocket socket) where T : BaseSocketClient, new()
        {
            BaseSocketClient client = new T();
            foreach (var apiClient in client.ApiClients.OfType<SocketApiClient>())
            {
                var factory = Mock.Of<IWebsocketFactory>();
                Mock.Get(factory).Setup(f => f.CreateWebsocket(It.IsAny<ILogger>(), It.IsAny<WebSocketParameters>())).Returns(socket);
                apiClient.SocketFactory = factory;
            }

            return (T)client;
        }

        public static T CreateRestClient<T>(string data, HttpStatusCode code) where T : BaseRestClient
        {
            BaseRestClient client = (T)Activator.CreateInstance(typeof(T), new object[] { null })!;
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

            return (T)client;
        }

        public static object? GetTestValue(Type type, int i)
        {
            if (type == typeof(bool))
                return true;

            if (type == typeof(bool?))
                return (bool?)true;

            if (type == typeof(decimal))
                return i / 100m;

            if (type == typeof(decimal?))
                return (decimal?)(i / 100m);

            if (type == typeof(int))
                return i;

            if (type == typeof(int?))
                return (int?)i;

            if (type == typeof(long))
                return (long)i;

            if (type == typeof(long?))
                return (long?)i;

            if (type == typeof(DateTime))
                return new DateTime(2019, 1, Math.Max(i, 1));

            if (type == typeof(DateTime?))
                return (DateTime?)new DateTime(2019, 1, Math.Max(i, 1));

            if (type == typeof(string))
                return "string" + i;

            if (type == typeof(IEnumerable<string>))
                return new[] { "string" + i };

            if (type.IsEnum)
                return Activator.CreateInstance(type)!;

            if (type.IsArray)
            {
                var elementType = type.GetElementType()!;
                var result = Array.CreateInstance(elementType, 2);
                result.SetValue(GetTestValue(elementType, 0), 0);
                result.SetValue(GetTestValue(elementType, 1), 1);
                return result;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var result = (IList)Activator.CreateInstance(type)!;
                result.Add(GetTestValue(type.GetGenericArguments()[0], 0));
                result.Add(GetTestValue(type.GetGenericArguments()[0], 1));
                return result;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var result = (IDictionary)Activator.CreateInstance(type)!;
                result.Add(GetTestValue(type.GetGenericArguments()[0], 0)!, GetTestValue(type.GetGenericArguments()[1], 0));
                result.Add(GetTestValue(type.GetGenericArguments()[0], 1)!, GetTestValue(type.GetGenericArguments()[1], 1));
                return Convert.ChangeType(result, type);
            }

            return null;
        }

        public static async Task<object> InvokeAsync(MethodInfo @this, object obj, params object[] parameters)
        {
            var task = (Task)@this.Invoke(obj, parameters)!;
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty!.GetValue(task)!;
        }
    }
}
