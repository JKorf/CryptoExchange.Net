using System;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Attribute for allowing specifying a JsonConverter with constructor parameters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonConverterCtorAttribute<T> : JsonConverterAttribute where T : JsonConverter
    {
        private readonly object[] _parameters;

        /// <summary>
        /// ctor
        /// </summary>
        public JsonConverterCtorAttribute(params object[] parameters) => _parameters = parameters;

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            return (T)Activator.CreateInstance(typeof(T), _parameters);
        }
    }

}
