using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    class NullableEnumConverterFactory : JsonConverterFactory
    {
        private readonly IJsonTypeInfoResolver jsonTypeInfoResolver;
        public NullableEnumConverterFactory(IJsonTypeInfoResolver jsonTypeInfoResolver)
        {
            this.jsonTypeInfoResolver = jsonTypeInfoResolver;
        }
        public override bool CanConvert(Type typeToConvert)
        {
            var b = Nullable.GetUnderlyingType(typeToConvert);
            if (b == null)
                return false;
            var typeInfo = jsonTypeInfoResolver.GetTypeInfo(b, new JsonSerializerOptions());
            if (typeInfo == null)
                return false;
            return typeInfo.Converter is INullableConverter;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var b = Nullable.GetUnderlyingType(typeToConvert);
            if (b == null)
                throw new ArgumentNullException($"Not nullable {typeToConvert.Name}");
            var typeInfo = jsonTypeInfoResolver.GetTypeInfo(b, new JsonSerializerOptions());
            if (typeInfo == null)
                throw new ArgumentNullException($"Can find type {typeToConvert.Name}");
            var t = typeInfo.Converter as INullableConverter;
            if (t == null)
            {
                throw new ArgumentNullException($"Can find type converter for {typeToConvert.Name}");
            }
            return t.CreateNullableConverter();
        }
    }
}
