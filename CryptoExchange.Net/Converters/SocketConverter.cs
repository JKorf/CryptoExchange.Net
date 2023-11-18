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
using System.Net.WebSockets;
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
        public BaseParsedMessage? ReadJson(WebSocketMessageType websocketMessageType, Stream stream, Dictionary<string, Type> processors, bool outputOriginalData)
        {
            // Start reading the data
            // Once we reach the properties that identify the message we save those in a dict
            // Once all id properties have been read callback to see what the deserialization type should be
            // Deserialize to the correct type

            if (InterpreterPipeline.PreProcessCallback != null)
                stream = InterpreterPipeline.PreProcessCallback(websocketMessageType, stream);

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
            catch(Exception ex)
            {
                // Not a json message
                return null;
            }

            var accessor = new JTokenAccessor(token);

            if (InterpreterPipeline.GetIdentity != null)
            {
                var identity = InterpreterPipeline.GetIdentity(accessor);
                if (identity != null)
                {
                    if (processors.TryGetValue(identity, out var type))
                    {
                        var idInstance = InterpreterPipeline.ObjectInitializer(token, type);
                        if (outputOriginalData)
                        {
                            stream.Position = 0;
                            idInstance.OriginalData = sr.ReadToEnd();
                        }

                        idInstance.Identifier = identity;
                        idInstance.Parsed = true;
                        return idInstance;
                    }
                }
            }

            PostInspectResult? inspectResult = null;
            object? usedParser = null;
            if (token.Type == JTokenType.Object)
            {
                foreach (var callback in InterpreterPipeline.PostInspectCallbacks.OfType<PostInspectCallback>())
                {
                    bool allFieldsPresent = true;
                    foreach (var field in callback.TypeFields)
                    {
                        var value = accessor.GetStringValue(field.Key);
                        if (value == null)
                        {
                            if (field.Required)
                            {
                                allFieldsPresent = false;
                                break;
                            }
                        }
                    }

                    if (allFieldsPresent)
                    {
                        inspectResult = callback.Callback(accessor, processors);
                        usedParser = callback;
                        if (inspectResult.Type != null)
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
            {
                //throw new Exception("No parser found for message");
                return null;
            }

            BaseParsedMessage instance;
            if (inspectResult.Type != null)
                instance = InterpreterPipeline.ObjectInitializer(token, inspectResult.Type);
            else
                instance = new ParsedMessage<object>(null);

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

            if (accessToken?.Type == JTokenType.Object)
                return ((JObject)accessToken).Properties().First().Name;

            return accessToken?.ToString();
        }
    }
}
