using CryptoExchange.Net.Objects.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CryptoExchange.Net.Converters
{
    public abstract class SocketConverter
    {
        public abstract string[] IdFields { get; }

        public abstract Type? GetDeserializationType(Dictionary<string, string> idValues, List<MessageListener> listeners);
        public abstract List<MessageListener> MatchToListener(ParsedMessage message, List<MessageListener> listeners);

        /// <inheritdoc />
        public object? ReadJson(Stream stream, List<MessageListener> listeners)
        {
            // Start reading the data
            // Once we reach the properties that identify the message we save those in a dict
            // Once all id properties have been read callback to see what the deserialization type should be
            // Deserialize to the correct type
            using var sr = new StreamReader(stream, Encoding.UTF8, false, (int)stream.Length, true);
            using var jsonTextReader = new JsonTextReader(sr);

            var token = JToken.Load(jsonTextReader);
            var dict = new Dictionary<string, string>();
            foreach(var idField in IdFields)
            {
                var splitTokens = idField.Split(new char[] { ':' });
                var accessToken = token;
                foreach (var splitToken in splitTokens)
                {
                    accessToken = accessToken[splitToken];
                }
                dict[idField] = accessToken?.ToString();
            }

            var resultType = GetDeserializationType(dict, listeners);
            string idString = "";
            foreach(var item in dict)
                idString += item.Value;

            return new ParsedMessage
            {
                Identifier = idString,
                Data = resultType == null ? null : token.ToObject(resultType)
            };
        }
    }

    public class ParsedMessage
    {
        public string Identifier { get; set; } = null!;

        public object? Data { get; set; }
    }
}
