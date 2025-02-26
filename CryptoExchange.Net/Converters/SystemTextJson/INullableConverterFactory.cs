using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    internal interface INullableConverterFactory
    {
        JsonConverter CreateNullableConverter();
    }
}
