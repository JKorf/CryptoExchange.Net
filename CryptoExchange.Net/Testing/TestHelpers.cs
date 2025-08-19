using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Testing.Implementations;

#pragma warning disable IL2026
#pragma warning disable IL2070
#pragma warning disable IL2075
#pragma warning disable IL3050

namespace CryptoExchange.Net.Testing
{
    /// <summary>
    /// Testing helpers
    /// </summary>
    public class TestHelpers
    {
        [ExcludeFromCodeCoverage]
        internal static bool AreEqual<T>(T? self, T? to, params string[] ignore) where T : class
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

        internal static TestSocket ConfigureSocketClient<T>(T client, string address) where T : BaseSocketClient
        {
            var socket = new TestSocket(address);
            foreach (var apiClient in client.ApiClients.OfType<SocketApiClient>())
            {
                apiClient.SocketFactory = new TestWebsocketFactory(socket);
            }
            return socket;
        }

        internal static void ConfigureRestClient<T>(T client, string data, HttpStatusCode code) where T : BaseRestClient
        {
            foreach (var apiClient in client.ApiClients.OfType<RestApiClient>())
            {
                var expectedBytes = Encoding.UTF8.GetBytes(data);
                var responseStream = new MemoryStream();
                responseStream.Write(expectedBytes, 0, expectedBytes.Length);
                responseStream.Seek(0, SeekOrigin.Begin);

                var response = new TestResponse(code, responseStream);
                var request = new TestRequest(response);

                var factory = new TestRequestFactory(request);
                apiClient.RequestFactory = factory;
            }
        }

        /// <summary>
        /// Check a signature matches the expected signature
        /// </summary>
        /// <param name="client"></param>
        /// <param name="authProvider"></param>
        /// <param name="method"></param>
        /// <param name="path"></param>
        /// <param name="getSignature"></param>
        /// <param name="expectedSignature"></param>
        /// <param name="parameters"></param>
        /// <param name="time"></param>
        /// <param name="disableOrdering"></param>
        /// <param name="compareCase"></param>
        /// <param name="host"></param>
        /// <exception cref="Exception"></exception>
        public static void CheckSignature(
            RestApiClient client,
            AuthenticationProvider authProvider,
            HttpMethod method,
            string path,
            Func<IDictionary<string, object>?, IDictionary<string, object>?, IDictionary<string, string>?, string> getSignature,
            string expectedSignature,
            Dictionary<string, object>? parameters = null,
            DateTime? time = null,
            bool disableOrdering = false,
            bool compareCase = true,
            string host = "https://test.test-api.com")
        {
            parameters ??= new Dictionary<string, object>
                {
                    { "test", 123 },
                    { "test2", "abc" }
                };

            if (disableOrdering)
                client.OrderParameters = false;

            var uriParams = client.ParameterPositions[method] == HttpMethodParameterPosition.InUri ? client.CreateParameterDictionary(parameters) : null;
            var bodyParams = client.ParameterPositions[method] == HttpMethodParameterPosition.InBody ? client.CreateParameterDictionary(parameters) : null;

            var headers = new Dictionary<string, string>();

            authProvider.TimeProvider = new TestAuthTimeProvider(time ?? new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc));
            authProvider.ProcessRequest(
                client,
                new RestRequestConfiguration(
                    new RequestDefinition(path, method)
                    {
                        Authenticated = true
                    },
                    host,
                    uriParams ?? new Dictionary<string, object>(),
                    bodyParams ?? new Dictionary<string, object>(),
                    headers,
                    client.ArraySerialization,
                    client.ParameterPositions[method],
                    client.RequestBodyFormat
                    )
                );

            var signature = getSignature(uriParams, bodyParams, headers);

            if (!string.Equals(signature, expectedSignature, compareCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Signatures do not match. Expected: {expectedSignature}, Actual: {signature}");
        }

        /// <summary>
        /// Scan the TClient rest client type for missing interface methods
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <exception cref="Exception"></exception>
        public static void CheckForMissingRestInterfaces<TClient>(string[]? excludeInterfaces = null)
        {
            CheckForMissingInterfaces(typeof(TClient), typeof(Task), excludeInterfaces);
        }

        /// <summary>
        /// Scan the TClient socket client type for missing interface methods
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <exception cref="Exception"></exception>
        public static void CheckForMissingSocketInterfaces<TClient>(string[]? excludeInterfaces = null)
        {
            CheckForMissingInterfaces(typeof(TClient), typeof(Task<CallResult<UpdateSubscription>>), excludeInterfaces);
        }

        private static void CheckForMissingInterfaces(Type clientType, Type implementationTypes, string[]? excludeInterfaces = null)
        {
            var assembly = Assembly.GetAssembly(clientType);
            var interfaceType = clientType.GetInterface("I" + clientType.Name);
            var clientInterfaces = assembly!.GetTypes()
                .Where(t => t.Name.StartsWith("I" + clientType.Name)
                        && !t.Name.EndsWith("Shared")
                        && (excludeInterfaces?.Contains(t.Name) != true));

            foreach (var clientInterface in clientInterfaces)
            {
                var implementations = assembly.GetTypes().Where(t => clientInterface.IsAssignableFrom(t) && !t.IsInterface && t != clientInterface);
                foreach (var implementation in implementations)
                {
                    int methods = 0;
                    foreach (var method in implementation.GetMethods().Where(m => implementationTypes.IsAssignableFrom(m.ReturnType)))
                    {
                        var interfaceMethod = 
                            clientInterface.GetMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray())
                            ?? clientInterface.GetInterfaces().Select(x => x.GetMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray())).FirstOrDefault()
                            ?? throw new Exception($"Missing interface for method {method.Name} in {implementation.Name} implementing interface {clientInterface.Name}");
                        methods++;
                    }

                    Debug.WriteLine($"{clientInterface.Name} {methods} methods validated");
                }
            }
        }
    }
}
