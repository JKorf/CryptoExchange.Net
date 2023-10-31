using CryptoExchange.Net.Objects.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Socket message converter
    /// </summary>
    public abstract class SocketConverter
    {
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
        public abstract Type? GetDeserializationType(Dictionary<string, string?> idValues, List<MessageListener> listeners);

        /// <inheritdoc />
        public ParsedMessage? ReadJson(Stream stream, List<MessageListener> listeners, bool outputOriginalData)
        {
            // Start reading the data
            // Once we reach the properties that identify the message we save those in a dict
            // Once all id properties have been read callback to see what the deserialization type should be
            // Deserialize to the correct type
            var result = new ParsedMessage();

            using var sr = new StreamReader(stream, Encoding.UTF8, false, (int)stream.Length, true);
            if (outputOriginalData)
            {
                result.OriginalData = sr.ReadToEnd();
                stream.Position = 0;
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

            var typeIdDict = new Dictionary<string, string?>();
            string idString = "";
            foreach (var idField in TypeIdFields)
            {
                var val = GetValueForKey(token, idField);
                idString += val;
                typeIdDict[idField] = val;
            }

            if (SubscriptionIdFields != null)
            {
                idString = "";
                foreach (var idField in SubscriptionIdFields)
                    idString += GetValueForKey(token, idField);
            }

            result.Identifier = idString;
            var resultType = GetDeserializationType(typeIdDict, listeners);
            result.Data = resultType == null ? null : token.ToObject(resultType);
            return result;
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
