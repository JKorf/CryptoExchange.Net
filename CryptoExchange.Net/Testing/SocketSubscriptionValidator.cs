using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
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
    public class SocketSubscriptionValidator<TClient> where TClient : BaseSocketClient
    {
        private readonly TClient _client;
        private readonly string _folder;
        private readonly string _baseAddress;
        private readonly string? _nestedPropertyForCompare;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="client">Client to test</param>
        /// <param name="folder">Folder for json test values</param>
        /// <param name="baseAddress">The base address that is expected</param>
        /// <param name="nestedPropertyForCompare">Property to use for compare</param>
        public SocketSubscriptionValidator(TClient client, string folder, string baseAddress, string? nestedPropertyForCompare = null)
        {
            _client = client;
            _folder = folder;
            _baseAddress = baseAddress;
            _nestedPropertyForCompare = nestedPropertyForCompare;
        }

        /// <summary>
        /// Validate a subscription
        /// </summary>
        /// <typeparam name="TUpdate">The expected update type</typeparam>
        /// <param name="methodInvoke">Subscription method invocation</param>
        /// <param name="name">Method name for looking up json test values</param>
        /// <param name="nestedJsonProperty">Use nested json property for compare</param>
        /// <param name="ignoreProperties">Ignore certain properties</param>
        /// <param name="useFirstUpdateItem">Use the first item of an array update</param>
        /// <param name="addressPath">Path</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ValidateAsync<TUpdate>(
            Func<TClient, Action<DataEvent<TUpdate>>, Task<CallResult<UpdateSubscription>>> methodInvoke,
            string name,
            string? nestedJsonProperty = null,
            List<string>? ignoreProperties = null,
            string? addressPath = null,
            bool? useFirstUpdateItem = null)
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

            var socket = TestHelpers.ConfigureSocketClient(_client, addressPath == null ? _baseAddress : _baseAddress.AppendPath(addressPath));

            var waiter = new AutoResetEvent(false);
            string? lastMessage = null;
            socket.OnMessageSend += (x) =>
            {
                lastMessage = x;
                waiter.Set();
            };

            TUpdate? update = default;
            // Invoke subscription method
            var task = methodInvoke(_client, x => { update = x.Data; });

            string? overrideKey = null;
            string? overrideValue = null;
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;

                if (line.StartsWith("> "))
                {
                    // Expect a message from client to server
                    waiter.WaitOne(TimeSpan.FromSeconds(1));

                    if (lastMessage == null)
                        throw new Exception($"{name} expected to {line} to be send to server but did not receive anything");

                    var lastMessageJson = JsonDocument.Parse(lastMessage).RootElement;
                    var expectedJson = JsonDocument.Parse(line.Substring(2)).RootElement;
                    if (expectedJson.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var item in expectedJson.EnumerateObject())
                        {
                            if (item.Value.ToString().StartsWith("|") && item.Value.ToString().EndsWith("|"))
                            {
                                // |x| values are used to replace parts or response messages
                                overrideKey = item.Value.ToString();
                                overrideValue = lastMessageJson.GetProperty(item.Name).GetString();
                            }
                            else if (item.Value.ToString() == "-999")
                            {
                                // -999 value is used to replace parts or response messages
                                overrideKey = item.Value.ToString();
                                overrideValue = lastMessageJson.GetProperty(item.Name).GetDecimal().ToString();
                            }
                            else if (lastMessageJson.GetProperty(item.Name).GetString() != item.Value.ToString() && ignoreProperties?.Contains(item.Name) != true)
                            {
                                throw new Exception($"{name} Expected {item.Name} to be {item.Value}, but was {lastMessageJson.GetProperty(item.Name).GetString()}");
                            }
                        }
                        // TODO check arrays and sub-objects

                    }
                }
                else if (line.StartsWith("< "))
                {
                    // Expect a message from server to client
                    if (overrideKey != null)
                    {
                        line = line.Replace(overrideKey, overrideValue);
                        overrideKey = null;
                        overrideValue = null;
                    }

                    socket.InvokeMessage(line.Substring(2));
                }
                else
                {
                    // A update message from server to client
                    var compareData = reader.ReadToEnd();
                    socket.InvokeMessage(compareData);

                    if (update == null)
                        throw new Exception($"{name} Update send to client did not trigger in update handler");

                    SystemTextJsonComparer.CompareData(name, update, compareData, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties, useFirstUpdateItem ?? false);
                }
            }

            await _client.UnsubscribeAllAsync().ConfigureAwait(false);
            Trace.Listeners.Remove(listener);
        }
    }
}
