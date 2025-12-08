using System;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    internal class NullableEnumConverterFactory : JsonConverterFactory
    {
        private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions();

        public NullableEnumConverterFactory(IJsonTypeInfoResolver jsonTypeInfoResolver)
        {
            _jsonTypeInfoResolver = jsonTypeInfoResolver;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            var b = Nullable.GetUnderlyingType(typeToConvert);
            if (b == null)
                return false;

            var typeInfo = _jsonTypeInfoResolver.GetTypeInfo(b, _options);
            if (typeInfo == null)
                return false;

            return typeInfo.Converter is INullableConverterFactory;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var b = Nullable.GetUnderlyingType(typeToConvert) ?? throw new ArgumentNullException($"Not nullable {typeToConvert.Name}");
            var typeInfo = _jsonTypeInfoResolver.GetTypeInfo(b, _options) ?? throw new ArgumentNullException($"Can find type {typeToConvert.Name}");
            if (typeInfo.Converter is not INullableConverterFactory nullConverterFactory)
                throw new ArgumentNullException($"Can find type converter for {typeToConvert.Name}");
            
            return nullConverterFactory.CreateNullableConverter();
        }
    }
}
