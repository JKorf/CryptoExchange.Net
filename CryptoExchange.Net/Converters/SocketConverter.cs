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
        public BaseParsedMessage? ReadJson(Stream stream, IDictionary<string, IMessageProcessor> processors, bool outputOriginalData)
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

            if (token.Type == JTokenType.Array)
            {
                // Received array, take first item as reference
                token = token.First!;
            }

            PostInspectResult? inspectResult = null;
            Dictionary<string, string> typeIdDict = new Dictionary<string, string>();
            PostInspectCallback? usedParser = null;
            foreach (var callback in InterpreterPipeline.PostInspectCallbacks)
            {
                bool allFieldsPresent = true;
                foreach(var field in callback.TypeFields)
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

            if (usedParser == null)
                throw new Exception("No parser found for message");

            var resultMessageType = typeof(ParsedMessage<>).MakeGenericType(inspectResult.Type);
            var instance = (BaseParsedMessage)Activator.CreateInstance(resultMessageType, inspectResult.Type == null ? null : token.ToObject(inspectResult.Type, _serializer));
            if (outputOriginalData)
            {
                stream.Position = 0;
                instance.OriginalData = sr.ReadToEnd();
            }

            instance.Identifier = inspectResult.Identifier;
            instance.Parsed = inspectResult.Type != null;
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
