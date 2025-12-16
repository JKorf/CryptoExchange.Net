using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Testing.Comparers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing
{
    /// <summary>
    /// Validator for websocket subscriptions, checking expected requests and responses and comparing update models
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public class SocketRequestValidator<TClient> where TClient : BaseSocketClient
    {
        private readonly string _baseAddress = "wss://localhost";
        private readonly string _folder;
        private readonly string? _nestedPropertyForCompare;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="folder">Folder for json test values</param>
        /// <param name="nestedPropertyForCompare">Property to use for compare</param>
        public SocketRequestValidator(string folder, string? nestedPropertyForCompare = null)
        {
            _folder = folder;
            _nestedPropertyForCompare = nestedPropertyForCompare;
        }

        /// <summary>
        /// Validate a subscription
        /// </summary>
        /// <typeparam name="TResponse">Expected response type</typeparam>
        /// <param name="client">Client to test</param>
        /// <param name="methodInvoke">Subscription method invocation</param>
        /// <param name="name">Method name for looking up json test values</param>
        /// <param name="responseMapper">Chose nested property to use for comparing</param>
        /// <param name="nestedJsonProperty">Use nested json property for compare</param>
        /// <param name="ignoreProperties">Ignore certain properties</param>
        /// <param name="useSingleArrayItem">Use the first item of an array update</param>
        /// <param name="skipResponseValidation">Whether to skip the response model validation</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ValidateAsync<TResponse>(
           TClient client,
           Func<TClient, Task<CallResult<TResponse>>> methodInvoke,
           string name,
           Func<TResponse, object>? responseMapper = null,
           string? nestedJsonProperty = null,
           List<string>? ignoreProperties = null,
           bool useSingleArrayItem = false,
           bool skipResponseValidation = false)
        {
            var listener = new EnumValueTraceListener();
            Trace.Listeners.Add(listener);
            
            var path = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
            FileStream file ;
            try
            {
                file = File.OpenRead(Path.Combine(path, _folder, $"{name}.txt"));
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Response file not found");
            }

            var buffer = new byte[file.Length];
            await file.ReadAsync(buffer, 0, (int)file.Length).ConfigureAwait(false);
            file.Close();

            var data = Encoding.UTF8.GetString(buffer);
            using var reader = new StringReader(data);

            var socket = TestHelpers.ConfigureSocketClient(client, _baseAddress);

            var waiter = new AutoResetEvent(false);
            string? lastMessage = null;
            socket.OnMessageSend += (x) =>
            {
                lastMessage = x;
                waiter.Set();
            };

            // Invoke subscription method
            var task = methodInvoke(client);

            var replaceValues = new Dictionary<string, string>();
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;

                if (line.StartsWith("> "))
                {
                    // Expect a message from client to server
                    waiter.WaitOne(TimeSpan.FromSeconds(5));

                    if (lastMessage == null)
                        throw new Exception($"{name} expected {line} to be send to server but did not receive anything");

                    var lastMessageJson = JsonDocument.Parse(lastMessage).RootElement;
                    var expectedJson = JsonDocument.Parse(line.Substring(2)).RootElement;
                    if (expectedJson.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var item in expectedJson.EnumerateObject())
                        {
                            if (item.Value.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var innerItem in item.Value.EnumerateObject())
                                {
                                    if (innerItem.Value.ToString().StartsWith("|") && innerItem.Value.ToString().EndsWith("|"))
                                    {
                                        // |x| values are used to replace parts of response messages
                                        if (!lastMessageJson.GetProperty(item.Name).TryGetProperty(innerItem.Name, out var prop))
                                            continue;

                                        replaceValues.Add(innerItem.Value.ToString(), prop.ValueKind == JsonValueKind.String ? prop.GetString()! : prop.GetInt64().ToString()!);
                                    }
                                }
                            }

                            if (item.Value.ToString().StartsWith("|") && item.Value.ToString().EndsWith("|"))
                            {
                                // |x| values are used to replace parts of response messages
                                if (!lastMessageJson.TryGetProperty(item.Name, out var prop))
                                    continue;

                                replaceValues.Add(item.Value.ToString(), prop.ValueKind == JsonValueKind.String ? prop.GetString()! : prop.GetInt64().ToString()!);
                            }
                            else if (!lastMessageJson.TryGetProperty(item.Name, out var prop))
                            {
                            }
                            else if (lastMessageJson.GetProperty(item.Name).ValueKind == JsonValueKind.String && lastMessageJson.GetProperty(item.Name).GetString() != item.Value.ToString() && ignoreProperties?.Contains(item.Name) != true)
                            {
                                throw new Exception($"{name} Expected {item.Name} to be {item.Value}, but was {lastMessageJson.GetProperty(item.Name).GetString()}");
                            }
                            else
                            {
                                // TODO check arrays and sub-objects

                            }
                        }
                        // TODO check arrays and sub-objects

                    }
                }
                else if (line.StartsWith("< "))
                {
                    // Expect a message from server to client
                    foreach(var item in replaceValues)
                        line = line.Replace(item.Key, item.Value);

                    socket.InvokeMessage(line.Substring(2));
                }
                else
                {
                    // A update message from server to client
                    var compareData = reader.ReadToEnd();
                    foreach (var item in replaceValues)
                        compareData = compareData.Replace(item.Key, item.Value);

                    socket.InvokeMessage(compareData);

                    await task.ConfigureAwait(false);
                    object? result = task.Result.Data;
                    if (responseMapper != null)
                        result = responseMapper(task.Result.Data);

                    if (!skipResponseValidation)
                        SystemTextJsonComparer.CompareData(name, result, compareData, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties, useSingleArrayItem);
                }
            }

            Trace.Listeners.Remove(listener);
        }
    }
}
