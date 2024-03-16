using System;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters.JsonNet
{
    /// <summary>
    /// Boolean converter with support for "0"/"1" (strings)
    /// </summary>
    public class BoolConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            if (Nullable.GetUnderlyingType(objectType) != null)
                return Nullable.GetUnderlyingType(objectType) == typeof(bool);
            return objectType == typeof(bool);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString().ToLower().Trim();
            if (value == null || value == "")
            {
                if (Nullable.GetUnderlyingType(objectType) != null)
                    return null;

                return false;
            }

            switch (value)
            {
                case "true":
                case "yes":
                case "y":
                case "1":
                case "on":
                    return true;
                case "false":
                case "no":
                case "n":
                case "0":
                case "off":
                case "-1":
                    return false;
            }

            // If we reach here, we're pretty much going to throw an error so let's let Json.NET throw it's pretty-fied error message.
            return new JsonSerializer().Deserialize(reader, objectType);
        }

        /// <summary>
        /// Specifies that this converter will not participate in writing results.
        /// </summary>
        public override bool CanWrite { get { return false; } }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
        }
    }
}