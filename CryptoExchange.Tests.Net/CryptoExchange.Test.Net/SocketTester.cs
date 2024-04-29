using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Test.Net
{
    public class SocketTester<TClient> where TClient : BaseSocketClient
    {
        private readonly TClient _client;
        private readonly string _folder;
        private readonly string _baseAddress;
        private readonly string? _nestedPropertyForCompare;
        private readonly bool _stjCompare;

        public SocketTester(TClient client, string folder, string baseAddress, string? nestedPropertyForCompare = null, bool stjCompare = true)
        {
            _client = client;
            _folder = folder;
            _baseAddress = baseAddress;
            _nestedPropertyForCompare = nestedPropertyForCompare;
            _stjCompare = stjCompare;
        }

        public async Task ValidateAsync<TResponse, TUpdate>(
            Func<TClient, Action<DataEvent<TUpdate>>, Task<CallResult<UpdateSubscription>>> methodInvoke,
            string name,
            string? nestedJsonProperty = null,
            List<string>? ignoreProperties = null)
        {
            var listener = new EnumValueTraceListener();
            Trace.Listeners.Add(listener);
            
            var path = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
            FileStream? file = null;
            try
            {
                file = File.OpenRead(Path.Combine(path, _folder, $"{name}.txt"));
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Response file not found");
            }

            var buffer = new byte[file.Length];
            await file.ReadAsync(buffer, 0, buffer.Length);
            file.Close();

            var data = Encoding.UTF8.GetString(buffer);
            using var reader = new StringReader(data);

            var socket = TestHelpers.ConfigureSocketClient(_client);

            var waiter = new AutoResetEvent(false);
            var lastMessage = "";
            socket.OnMessageSend += (x) =>
            {
                lastMessage = x;
                waiter.Set();
            };
            TUpdate update = default;
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
                    // Receive
                    waiter.WaitOne();
                    var lastMessageJson = JToken.Parse(lastMessage);
                    var expectedJson = JToken.Parse(line.Substring(2));
                    foreach(var item in expectedJson)
                    {
                        if (item is JProperty prop && prop.Value is JValue val)
                        {
                            if (val.ToString().StartsWith("|") && val.ToString().EndsWith("|"))
                            {
                                overrideKey = val.ToString();
                                overrideValue = lastMessageJson[prop.Name]?.Value<string>();
                            }
                            else if (lastMessageJson[prop.Name]?.Value<string>() != val.ToString() && ignoreProperties?.Contains(prop.Name) != true)
                                throw new Exception($"Expected {prop.Name} to be {val}, but was {lastMessageJson[prop.Name]?.Value<string>()}");
                        }

                        // TODO check objects and arrays
                    }
                }
                else if (line.StartsWith("< "))
                {
                    // Send
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
                    var compareData = reader.ReadToEnd();
                    socket.InvokeMessage(compareData);

                    if (update == null)
                        throw new Exception("No update processed");


                    if (_stjCompare == true)
                        SystemTextJsonComparer.CompareData(name, update, compareData, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties);
                    else
                        JsonNetComparer.CompareData(name, update, compareData, nestedJsonProperty ?? _nestedPropertyForCompare, ignoreProperties);
                }
            }

            await _client.UnsubscribeAllAsync();
            Trace.Listeners.Remove(listener);
        }
    }
}
