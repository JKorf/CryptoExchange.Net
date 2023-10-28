using CryptoExchange.Net.Objects.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Converters
{
    public class SocketConverter : JsonConverter
    {
        private readonly List<string> _idFields;
        private readonly Func<Dictionary<string, string>, Type> _typeIdentifier;

        public SocketConverter(List<string> idFields, Func<Dictionary<string, string>, Type> typeIdentifier)
        {
            _idFields = idFields;
            _typeIdentifier = typeIdentifier;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => true;

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Start reading the data
            // Once we reach the property that identifies the message we save those in a string array
            // Once all id properties have been read callback to see what the deserialization type should be
            // Deserialize to the correct type

            var token = JToken.Load(reader);
            var dict = new Dictionary<string, string>();
            foreach(var idField in _idFields)
            {
                var splitTokens = idField.Split(new char[] { ':' });
                var accessToken = token;
                foreach (var splitToken in splitTokens)
                {
                    accessToken = accessToken[splitToken];
                }
                dict[idField] = accessToken?.ToString();
            }

            var resultType = _typeIdentifier(dict);
            string idString = "";
            foreach(var item in dict)
                idString += item.Value;

            return new ParsedMessage
            {
                Identifier = idString,
                Data = resultType == null ? null : token.ToObject(resultType)
            };
        }

        /// <inheritdoc />
        public override bool CanWrite { get { return false; } }
                
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
        }
    }

    public class ParsedMessage
    {
        public string Identifier { get; set; }

        public object Data { get; set; }
    }
}
