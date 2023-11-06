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

        public abstract List<StreamMessageParseCallback> InterpreterPipeline { get; }

        public virtual string CreateIdentifierString(Dictionary<string, string?> idValues) => string.Join("-", idValues.Values.Where(v => v != null).Select(v => v!.ToLower()));

        /// <inheritdoc />
        public BaseParsedMessage? ReadJson(Stream stream, ConcurrentList<BasePendingRequest> pendingRequests, ConcurrentList<Subscription> listeners, bool outputOriginalData)
        {
            // Start reading the data
            // Once we reach the properties that identify the message we save those in a dict
            // Once all id properties have been read callback to see what the deserialization type should be
            // Deserialize to the correct type

            using var sr = new StreamReader(stream, Encoding.UTF8, false, (int)stream.Length, true);
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

            Type? resultType = null;
            Dictionary<string, string> typeIdDict = new Dictionary<string, string>();
            StreamMessageParseCallback? usedParser = null;
            foreach (var callback in InterpreterPipeline)
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
                    resultType = callback.Callback(typeIdDict, pendingRequests, listeners);
                    usedParser = callback;
                    break;
                }
            }

            if (usedParser == null)
                throw new Exception("No parser found for message");

            var subIdDict = new Dictionary<string, string?>();
            foreach (var field in usedParser.IdFields)
                subIdDict[field] = typeIdDict.TryGetValue(field, out var cachedValue) ? cachedValue : GetValueForKey(token, field);
        
            var resultMessageType = typeof(ParsedMessage<>).MakeGenericType(resultType);
            var instance = (BaseParsedMessage)Activator.CreateInstance(resultMessageType, resultType == null ? null : token.ToObject(resultType, _serializer));
            if (outputOriginalData)
            {
                stream.Position = 0;
                instance.OriginalData = sr.ReadToEnd();
            }

            instance.Identifier = CreateIdentifierString(subIdDict);
            instance.Parsed = resultType != null;
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
