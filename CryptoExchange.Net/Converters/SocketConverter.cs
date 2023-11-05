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

        /// <summary>
        /// Fields to use for the message subscription identifier
        /// </summary>
        public virtual string[]? SubscriptionIdFields => null;
        /// <summary>
        /// Fields to use for the message type identifier
        /// </summary>
        public abstract string[] TypeIdFields { get; }

        /// <summary>
        /// Return the type of object that the message should be parsed to based on the type id values dictionary
        /// </summary>
        /// <param name="idValues"></param>
        /// <param name="listeners"></param>
        /// <returns></returns>
        public abstract Type? GetDeserializationType(Dictionary<string, string?> idValues, List<BasePendingRequest> pendingRequests, List<Subscription> listeners);

        public virtual string CreateIdentifierString(Dictionary<string, string?> idValues) => string.Join("-", idValues.Values.Where(v => v != null).Select(v => v!.ToLower()));

        /// <inheritdoc />
        public BaseParsedMessage? ReadJson(Stream stream, List<BasePendingRequest> pendingRequests, List<Subscription> listeners, bool outputOriginalData)
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

            var typeIdDict = new Dictionary<string, string?>();
            foreach (var idField in TypeIdFields)
                typeIdDict[idField] = GetValueForKey(token, idField);

            Dictionary<string, string?>? subIdDict = null;
            if (SubscriptionIdFields != null)
            {
                subIdDict = new Dictionary<string, string?>();
                foreach (var idField in SubscriptionIdFields)
                    subIdDict[idField] = GetValueForKey(token, idField);
            }

            var resultType = GetDeserializationType(typeIdDict, pendingRequests, listeners);
            if (resultType == null)
            {
                // ?
                return null;
            }

            var resultMessageType = typeof(ParsedMessage<>).MakeGenericType(resultType);
            var instance = (BaseParsedMessage)Activator.CreateInstance(resultMessageType, resultType == null ? null : token.ToObject(resultType, _serializer));
            if (outputOriginalData)
            {
                stream.Position = 0;
                instance.OriginalData = sr.ReadToEnd();
            }

            instance.Identifier = CreateIdentifierString(subIdDict ?? typeIdDict);
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
