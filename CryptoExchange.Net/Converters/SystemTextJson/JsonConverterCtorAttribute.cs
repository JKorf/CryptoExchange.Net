using System;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Attribute for allowing specifying a JsonConverter with constructor parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonConverterCtorAttribute : JsonConverterAttribute
    {
        private readonly object[] _parameters;
        private readonly Type _type;

        /// <summary>
        /// ctor
        /// </summary>
        public JsonConverterCtorAttribute(Type type, params object[] parameters)
        {
            _type = type;
            _parameters = parameters;
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            return (JsonConverter)Activator.CreateInstance(_type, _parameters);
        }
    }

}
