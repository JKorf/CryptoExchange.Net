using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Socket message converter
    /// </summary>
    public abstract class SocketConverter
    {
        private static JsonSerializer _serializer = JsonSerializer.Create(SerializerOptions.WithConverters);

        public abstract MessageInterpreterPipeline InterpreterPipeline { get; }

        /// <inheritdoc />
        public BaseParsedMessage? ReadJson(Stream stream, Dictionary<string, Type> processors, bool outputOriginalData)
        {
            // Start reading the data
            // Once we reach the properties that identify the message we save those in a dict
            // Once all id properties have been read callback to see what the deserialization type should be
            // Deserialize to the correct type

            using var sr = new StreamReader(stream, Encoding.UTF8, false, (int)stream.Length, true);
            foreach (var callback in InterpreterPipeline.PreInspectCallbacks)
            {
                var result = callback.Callback(stream);
                if (result.Matched)
                {
                    var data = sr.ReadToEnd();
                    var messageType = typeof(ParsedMessage<>).MakeGenericType(typeof(string));
                    var preInstance = (BaseParsedMessage)Activator.CreateInstance(messageType, data);
                    if (outputOriginalData)
                    {
                        stream.Position = 0;
                        preInstance.OriginalData = data;
                    }

                    preInstance.Identifier = result.Identifier;
                    preInstance.Parsed = true;
                    return preInstance;
                }
            }

            using var jsonTextReader = new JsonTextReader(sr);
            JToken token;
            try
            {
                token = JToken.Load(jsonTextReader);
            }
            catch(Exception)
            {
                // Not a json message
                return null;
            }

            PostInspectResult? inspectResult = null;
            Dictionary<string, string> typeIdDict = new Dictionary<string, string>();
            object? usedParser = null;
            if (token.Type == JTokenType.Object)
            {
                foreach (var callback in InterpreterPipeline.PostInspectCallbacks.OfType<PostInspectCallback>())
                {
                    bool allFieldsPresent = true;
                    foreach (var field in callback.TypeFields)
                    {
                        var value = typeIdDict.TryGetValue(field, out var cachedValue) ? cachedValue : GetValueForKey(token, field);
                        if (value == null)
                        {
                            allFieldsPresent = false;
                            break;
                        }

                        typeIdDict[field] = value;
                    }

                    if (allFieldsPresent)
                    {
                        inspectResult = callback.Callback(typeIdDict, processors);
                        usedParser = callback;
                        break;
                    }
                }
            }
            else
            {
                foreach (var callback in InterpreterPipeline.PostInspectCallbacks.OfType<PostInspectArrayCallback>())
                {
                    var typeIdArrayDict = new Dictionary<int, string>();
                    bool allFieldsPresent = true;
                    var maxIndex = callback.TypeFields.Max();
                    if (((JArray)token).Count <= maxIndex)
                        continue;

                    foreach (var field in callback.TypeFields)
                    {
                        var value = token[field];
                        if (value == null)
                        {
                            allFieldsPresent = false;
                            break;
                        }

                        typeIdArrayDict[field] = value.ToString();
                    }

                    if (allFieldsPresent)
                    {
                        inspectResult = callback.Callback(typeIdArrayDict, processors);
                        usedParser = callback;
                        break;
                    }
                }
            }

            if (usedParser == null)
                throw new Exception("No parser found for message");

            var instance = InterpreterPipeline.ObjectInitializer(token, inspectResult.Type);
            if (outputOriginalData)
            {
                stream.Position = 0;
                instance.OriginalData = sr.ReadToEnd();
            }

            instance.Identifier = inspectResult.Identifier;
            instance.Parsed = inspectResult.Type != null;
            return instance;
        }

        public static BaseParsedMessage InstantiateMessageObject(JToken token, Type type)
        {
            var resultMessageType = typeof(ParsedMessage<>).MakeGenericType(type);
            var instance = (BaseParsedMessage)Activator.CreateInstance(resultMessageType, type == null ? null : token.ToObject(type, _serializer));
            return instance;
        }

        private string? GetValueForKey(JToken token, string key)
        {
            var splitTokens = key.Split(new char[] { ':' });
            var accessToken = token;
            foreach (var splitToken in splitTokens)
            {
                accessToken = accessToken[splitToken];

                if (accessToken == null)
                    break;

                if (accessToken.Type == JTokenType.Array)
                {
                    // Received array, take first item as reference
                    accessToken = accessToken.First!;
                }
            }

            return accessToken?.ToString();
        }
    }
}
